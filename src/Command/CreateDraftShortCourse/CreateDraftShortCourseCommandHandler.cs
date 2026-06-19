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

public class CreateDraftShortCourseCommandHandler : ICommandHandler<CreateDraftShortCourseCommand, CreateDraftShortCourseCommandResult?>
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

    public async Task<CreateDraftShortCourseCommandResult?> Handle(CreateDraftShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftShortCourseCommand");

        var (learner, personalDetailsChanged) = await GetOrCreateLearner(command);

        var learning = await _shortCourseLearningRepository.GetByLearnerKeyAndCourseCode(learner.Key, command.Model.OnProgramme.CourseCode);

        var ukprn = command.Model.OnProgramme.Ukprn;

        //  Create if learning does not exist
        if (learning == null)
        {
            learning = CreateNewLearning(command, learner);

            if (personalDetailsChanged)
                learning.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, learning.LatestEpisodeForProvider(ukprn)));

            await _shortCourseLearningRepository.Add(learning);

            return new CreateDraftShortCourseCommandResult { LearningKey = learning.Key, LearnerKey = learner.Key, EpisodeKey = learning.Episodes.Single().Key };
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
            AddEpisode(learning, command);
        }
        else
        {
            updateResult = learning.Update(command.Model);
        }

        var episode = learning.Episodes.Single(e => e.Ukprn == command.Model.OnProgramme.Ukprn);
        if (personalDetailsChanged)
            learning.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, episode));

        await _shortCourseLearningRepository.Update(learning);
        
        var result = _mapper.Map<CreateDraftShortCourseCommandResult>(learning, learner, command.Model.OnProgramme.Ukprn);
        result.EpisodeKey = learning.Episodes.Single(x => x.Ukprn == command.Model.OnProgramme.Ukprn).Key;
        if(updateResult != null) result.IsReinstated = updateResult.Changes.Any(x => x == ShortCourseUpdateChanges.Reinstated);

        return result;
    }

    private void AddEpisode(ShortCourseLearningDomainModel learning, CreateDraftShortCourseCommand command)
    {
        var episode = learning.AddEpisode(
            command.Model.OnProgramme.Ukprn,
            command.Model.OnProgramme.EmployerId,
            command.Model.LearnerRef,
            command.Model.OnProgramme.CourseCode,
            false,
            command.Model.OnProgramme.StartDate,
            command.Model.OnProgramme.ExpectedEndDate,
            command.Model.OnProgramme.WithdrawalDate,
            command.Model.OnProgramme.WithdrawalReasonCode,
            command.Model.OnProgramme.Milestones,
            command.Model.OnProgramme.Price,
            command.Model.OnProgramme.LearningType,
            completionDate: command.Model.OnProgramme.CompletionDate);

        foreach (var learningSupport in command.Model.LearningSupport)
        {
            episode.AddLearningSupport(learningSupport.StartDate, learningSupport.EndDate);
        }
    }

    private ShortCourseLearningDomainModel CreateNewLearning(CreateDraftShortCourseCommand command, LearnerDomainModel learner)
    {
        var learning = _shortCourseLearningFactory.CreateNew(learner.Key, command.Model.OnProgramme.CourseCode);

        AddEpisode(learning, command);

        return learning;
    }

    private async Task<(LearnerDomainModel, bool)> GetOrCreateLearner(CreateDraftShortCourseCommand command)
    {
        var personalDetailsChanged = false;
        var learner = await _learnerRepository.GetByUln(command.Model.Learner.Uln);

        if (learner != null)
        {
            var updateContext = new LearningUpdateContext()
            {
                Learner = new LearnerModel
                {
                    FirstName = command.Model.Learner.FirstName,
                    LastName = command.Model.Learner.LastName,
                    DateOfBirth = command.Model.Learner.DateOfBirth,
                    EmailAddress = command.Model.Learner.EmailAddress
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
            command.Model.Learner.Uln, 
            command.Model.Learner.DateOfBirth, 
            command.Model.Learner.FirstName, 
            command.Model.Learner.LastName,
            command.Model.Learner.EmailAddress);

        await _learnerRepository.Add(newLearner);

        personalDetailsChanged = true;

        return (newLearner, personalDetailsChanged);
    }

}