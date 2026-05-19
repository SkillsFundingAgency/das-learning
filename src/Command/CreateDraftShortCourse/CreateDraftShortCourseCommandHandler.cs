using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

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

        var learner = await GetOrCreateLearner(command);

        var learning = await _shortCourseLearningRepository.GetByLearnerKey(learner.Key);

        //  Create if learning does not exist
        if(learning == null)
        {
            learning = CreateNewLearning(command, learner);

            TransferEvents(learner, learning);
            await _shortCourseLearningRepository.Add(learning);

            return new CreateDraftShortCourseCommandResult { LearningKey = learning.Key, EpisodeKey = learning.LatestEpisodeForProvider(command.Model.OnProgramme.Ukprn).Key };
        }

        var isReinstated = false;

        if (!_featureFlags.ShortCourseChangeOfProvider)
        {
            if (learning.Episodes.Any(x => x.Ukprn != command.Model.OnProgramme.Ukprn))
            {
                _logger.LogWarning("An episode with a different provider already exists for learner with key {LearnerKey}. Cannot create draft when short course change of provider feature is disabled.", learner.Key);
                return null;
            }

            if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved))
            {
                _logger.LogWarning("An approved short course episode already exists for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
                return null;
            }

            if (!learning.Episodes.Any())
            {
                AddEpisode(learning, command);
            }
            else
            {
                learning.Update(command.Model);

                if (learning.Episodes.Any(x => x.IsRemoved))
                {
                    learning.Reinstate(command.Model.OnProgramme.Ukprn);
                    isReinstated = true;
                }
            }
        }
        else
        {
            // Ignore if provider posts a short course when they already have an approved short course
            if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved && x.Ukprn == command.Model.OnProgramme.Ukprn))
            {
                _logger.LogWarning("An approved short course episode already exists with this provider for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
                return null;
            }

            // If no episode exists for this provider, add a new one
            if (!learning.Episodes.Any(x => x.Ukprn == command.Model.OnProgramme.Ukprn))
            {
                AddEpisode(learning, command);
            }
            else
            {
                // Update existing learning if same provider
                learning.Update(command.Model);

                // Reinstate learner if provider's episode has previously been removed
                if (learning.Episodes.Any(x => x.Ukprn == command.Model.OnProgramme.Ukprn && x.IsRemoved))
                {
                    learning.Reinstate(command.Model.OnProgramme.Ukprn);
                    isReinstated = true;
                }
            }
        }

        TransferEvents(learner, learning);
        await _shortCourseLearningRepository.Update(learning);

        var result = _mapper.Map<CreateDraftShortCourseCommandResult>(learning, learner, command.Model.OnProgramme.Ukprn);
        result.EpisodeKey = learning.Episodes.Single(x => x.Ukprn == command.Model.OnProgramme.Ukprn).Key;
        result.IsReinstated = isReinstated;
        return result;
    }

    private void AddEpisode(ShortCourseLearningDomainModel learning, CreateDraftShortCourseCommand command)
    {
        var episode = learning.AddEpisode(
            command.Model.OnProgramme.Ukprn,
            command.Model.OnProgramme.EmployerId,
            command.Model.OnProgramme.CourseCode,
            command.Model.LearnerRef,
            false,
            command.Model.OnProgramme.StartDate,
            command.Model.OnProgramme.ExpectedEndDate,
            command.Model.OnProgramme.WithdrawalDate,
            command.Model.OnProgramme.Milestones,
            command.Model.OnProgramme.Price,
            command.Model.OnProgramme.LearningType);

        foreach (var learningSupport in command.Model.LearningSupport)
        {
            episode.AddLearningSupport(learningSupport.StartDate, learningSupport.EndDate);
        }
    }

    private ShortCourseLearningDomainModel CreateNewLearning(CreateDraftShortCourseCommand command, LearnerDomainModel learner)
    {
        var learning = _shortCourseLearningFactory.CreateNew(
            learner.Key,
            command.Model.OnProgramme.CompletionDate);

        AddEpisode(learning, command);

        return learning;
    }

    private async Task<LearnerDomainModel> GetOrCreateLearner(CreateDraftShortCourseCommand command)
    {
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

            learner.Update(updateContext);

            return learner;

        }

        var newLearner = _learnerFactory.CreateNew(
            command.Model.Learner.Uln, 
            command.Model.Learner.DateOfBirth, 
            command.Model.Learner.FirstName, 
            command.Model.Learner.LastName,
            command.Model.Learner.EmailAddress);

        await _learnerRepository.Add(newLearner);
        return newLearner;
    }

    /// <summary>
    /// TEMPORARY WORKAROUND
    /// 
    /// Because our repositories share a dbcontext, it is enough to update one of the 
    /// aggregates for changes across both to be persisted. 
    /// However, events are stored against the aggregate that raised them, so we need to transfer any events raised 
    /// on the learner to the learning aggregate before saving so they get persisted and dispatched.
    /// </summary>
    private static void TransferEvents(LearnerDomainModel learner, ShortCourseLearningDomainModel learning)
    {
        var learnerEvents = learner.FlushEvents();
        foreach (var domainEvent in learnerEvents)
        {
            learning.AddEvent(domainEvent);
        }
    }
}