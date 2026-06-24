using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System.Threading.Channels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandHandler : ICommandHandler<CreateDraftShortCourseCommand, CreateDraftShortCourseCommandResponse>
{
    private readonly ILearnerFactory _learnerFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IShortCourseLearningRepository _shortCourseLearningRepository;
    private readonly IShortCourseLearningFactory _shortCourseLearningFactory;
    private readonly IShortCourseLearningDomainModelMapper _mapper;
    private readonly ILogger<CreateDraftShortCourseCommandHandler> _logger;
    private readonly FeatureFlags _featureFlags;

    public CreateDraftShortCourseCommandHandler(
        ILearnerFactory learnerFactory,
        ILearnerRepository learnerRepository,
        IShortCourseLearningRepository shortCourseLearningRepository,
        IShortCourseLearningFactory shortCourseLearningFactory,
        IShortCourseLearningDomainModelMapper mapper,
        ILogger<CreateDraftShortCourseCommandHandler> logger,
        FeatureFlags featureFlags)
    {
        _learnerFactory = learnerFactory;
        _learnerRepository = learnerRepository;
        _shortCourseLearningRepository = shortCourseLearningRepository;
        _shortCourseLearningFactory = shortCourseLearningFactory;
        _mapper = mapper;
        _logger = logger;
        _featureFlags = featureFlags;
    }

    public async Task<CreateDraftShortCourseCommandResponse> Handle(CreateDraftShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftShortCourseCommand");

        var (learner, personalDetailsChanged) = await GetOrCreateLearner(command);

        var existingLearnings = await _shortCourseLearningRepository.GetAllByLearnerKey(learner.Key);
        var learnerHasExistingLearnings = existingLearnings.Count > 0;

        var results = new List<CreateDraftShortCourseCommandResult>();
        var processedLearningKeys = new HashSet<Guid>();

        foreach (var model in command.Models)
        {
            var result = await HandleSingleItem(model, learner, personalDetailsChanged, learnerHasExistingLearnings);
            if (result != null)
            {
                results.Add(result);
                if (!result.IsIgnored)
                {
                    processedLearningKeys.Add(result.LearningKey);
                    learnerHasExistingLearnings = true;
                }
            }
        }

        if (_featureFlags.ShortCourseProgression)
        {
            var requestedCourseCodes = command.Models.Select(m => m.OnProgramme.CourseCode).ToHashSet();
            await RemoveOmittedLearnings(command, existingLearnings, results, processedLearningKeys, requestedCourseCodes);
        }

        return new CreateDraftShortCourseCommandResponse { Results = results };
    }

    private async Task RemoveOmittedLearnings(CreateDraftShortCourseCommand command, List<ShortCourseLearningDomainModel> existingLearnings, List<CreateDraftShortCourseCommandResult> results, HashSet<Guid> processedLearningKeys, HashSet<string> requestedCourseCodes)
    {
        foreach (var learning in existingLearnings.Where(l => !processedLearningKeys.Contains(l.Key) && !requestedCourseCodes.Contains(l.TrainingCode)))
        {
            var activeEpisode = learning.Episodes.SingleOrDefault(e => e.Ukprn == command.Ukprn && !e.IsRemoved);
            if (activeEpisode == null)
                continue;

            var removedEpisodeKey = learning.Remove(command.Ukprn);

            if (!removedEpisodeKey.HasValue)
                continue;

            await _shortCourseLearningRepository.Update(learning);

            _logger.LogInformation("Removed omitted Learning {LearningKey} / {CourseCode} for LearnerKey {LearnerKey}",
                learning.Key, learning.TrainingCode, learning.LearnerKey);

            results.Add(new CreateDraftShortCourseCommandResult
            {
                IsRemoved = true,
                LearningKey = learning.Key,
                CourseCode = learning.TrainingCode,
                EpisodeKey = removedEpisodeKey.Value
            });
        }
    }

    private async Task<CreateDraftShortCourseCommandResult?> HandleSingleItem(ShortCourseUpdateContext model, LearnerDomainModel learner, bool personalDetailsChanged, bool learnerHasExistingLearnings)
    {
        var ukprn = model.OnProgramme.Ukprn;

        var learning = await _shortCourseLearningRepository.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode);

        //  Create if learning does not exist for this CourseCode
        if (learning == null)
        {
            // Learner already has at least one Learning for a different CourseCode - this is Progression, gated behind the feature flag.
            if (learnerHasExistingLearnings && !_featureFlags.ShortCourseProgression)
            {
                _logger.LogInformation(
                    "No learning found for LearnerKey {LearnerKey} / CourseCode {CourseCode} and learner already has other learnings; Short Course Progression is disabled — ignoring",
                    learner.Key, model.OnProgramme.CourseCode);
                return new CreateDraftShortCourseCommandResult { IsIgnored = true };
            }

            var newLearning = CreateNewLearning(model, learner);

            if (personalDetailsChanged)
                newLearning.AddEvent(PersonalDetailsChangedEvent.From(learner, newLearning, newLearning.LatestEpisodeForProvider(ukprn)));

            await _shortCourseLearningRepository.Add(newLearning);

            return new CreateDraftShortCourseCommandResult { LearningKey = newLearning.Key, LearnerKey = learner.Key, EpisodeKey = newLearning.Episodes.Single().Key };
        }

        if (!_featureFlags.ShortCourseChangeOfProvider)
        {
            if (learning.Episodes.Any(x => x.Ukprn != ukprn))
            {
                _logger.LogWarning("An episode with a different provider already exists for learner with key {LearnerKey}. Cannot create draft when short course change of provider feature is disabled.", learner.Key);
                return null;
            }

            if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved))
            {
                _logger.LogWarning("An approved short course episode already exists for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
                return null;
            }
        }
        else
        {
            if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved && x.Ukprn == ukprn))
            {
                _logger.LogWarning("An approved short course episode already exists with this provider for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
                return null;
            }
        }

        var existingEpisode = _featureFlags.ShortCourseChangeOfProvider
            ? learning.Episodes.Any(x => x.Ukprn == ukprn)
            : learning.Episodes.Any();

        ShortCourseDomainUpdateResult? updateResult = null;

        if (!existingEpisode)
        {
            AddEpisode(learning, model);
        }
        else
        {
            updateResult = learning.Update(model);
        }

        var episode = learning.Episodes.Single(e => e.Ukprn == ukprn);
        if (personalDetailsChanged)
            learning.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, episode));

        await _shortCourseLearningRepository.Update(learning);

        var result = _mapper.Map<CreateDraftShortCourseCommandResult>(learning, learner, ukprn);
        result.EpisodeKey = learning.Episodes.Single(x => x.Ukprn == ukprn).Key;
        if (updateResult != null) result.IsReinstated = updateResult.Changes.Any(x => x == ShortCourseUpdateChanges.Reinstated);

        return result;
    }

    private void AddEpisode(ShortCourseLearningDomainModel learning, ShortCourseUpdateContext model)
    {
        var episode = learning.AddEpisode(
            model.OnProgramme.Ukprn,
            model.OnProgramme.EmployerId,
            model.LearnerRef,
            model.OnProgramme.CourseCode,
            false,
            model.OnProgramme.StartDate,
            model.OnProgramme.ExpectedEndDate,
            model.OnProgramme.WithdrawalDate,
            model.OnProgramme.WithdrawalReasonCode,
            model.OnProgramme.Milestones,
            model.OnProgramme.Price,
            model.OnProgramme.LearningType,
            completionDate: model.OnProgramme.CompletionDate);

        foreach (var learningSupport in model.LearningSupport)
        {
            episode.AddLearningSupport(learningSupport.StartDate, learningSupport.EndDate);
        }
    }

    private ShortCourseLearningDomainModel CreateNewLearning(ShortCourseUpdateContext model, LearnerDomainModel learner)
    {
        var learning = _shortCourseLearningFactory.CreateNew(learner.Key, model.OnProgramme.CourseCode);

        AddEpisode(learning, model);

        return learning;
    }

    private async Task<(LearnerDomainModel, bool)> GetOrCreateLearner(CreateDraftShortCourseCommand command)
    {
        var personalDetailsChanged = false;
        var learnerModel = command.Models.First().Learner;
        var learner = await _learnerRepository.GetByUln(learnerModel.Uln);

        if (learner != null)
        {
            var updateContext = new LearningUpdateContext()
            {
                Learner = new LearnerModel
                {
                    FirstName = learnerModel.FirstName,
                    LastName = learnerModel.LastName,
                    DateOfBirth = learnerModel.DateOfBirth,
                    EmailAddress = learnerModel.EmailAddress
                },
                Care = new CareDetails // We need to pass in the same details so they don't get overwritten
                {
                    HasEHCP = learner.HasEHCP,
                    IsCareLeaver = learner.IsCareLeaver,
                    CareLeaverEmployerConsentGiven = learner.CareLeaverEmployerConsentGiven
                }
            };

            var changes = learner.Update(updateContext);
            await _learnerRepository.Update(learner);
            personalDetailsChanged = changes.Any(x=> x == LearningUpdateChanges.PersonalDetails);

            return (learner, personalDetailsChanged);

        }

        var newLearner = _learnerFactory.CreateNew(
            learnerModel.Uln,
            learnerModel.DateOfBirth,
            learnerModel.FirstName,
            learnerModel.LastName,
            learnerModel.EmailAddress);

        await _learnerRepository.Add(newLearner);

        personalDetailsChanged = true;

        return (newLearner, personalDetailsChanged);
    }

}