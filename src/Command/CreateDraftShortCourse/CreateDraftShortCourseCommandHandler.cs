using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
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

    public CreateDraftShortCourseCommandHandler(
        ILearnerFactory learnerFactory,
        ILearnerRepository learnerRepository,
        IShortCourseLearningRepository shortCourseLearningRepository,
        IShortCourseLearningFactory shortCourseLearningFactory,
        IShortCourseLearningDomainModelMapper mapper,
        ILogger<CreateDraftShortCourseCommandHandler> logger)
    {
        _learnerFactory = learnerFactory;
        _learnerRepository = learnerRepository;
        _shortCourseLearningRepository = shortCourseLearningRepository;
        _shortCourseLearningFactory = shortCourseLearningFactory;
        _mapper = mapper;
        _logger = logger;
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

            await _shortCourseLearningRepository.Add(learning);

            return new CreateDraftShortCourseCommandResult { LearningKey = learning.Key, EpisodeKey = learning.LatestEpisode.Key};
        }

        //  Ignore if we have an approved episode with another provider
        if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved && x.Ukprn != command.Model.OnProgramme.Ukprn))
        {
            _logger.LogWarning("An approved short course episode already exists with another provider for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
            return null;
        }

        //Ignore if provider posts a short course when they already have an approved short course
        if (learning.Episodes.Any(x => x.IsApproved && !x.IsRemoved && x.Ukprn == command.Model.OnProgramme.Ukprn))
        {
            _logger.LogWarning("An approved short course episode already exists with this provider for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
            return null;
        }

        //Ignore if we already have an unapproved short course with another provider
        if(learning.Episodes.Any(x => !x.IsApproved && !x.IsRemoved && x.Ukprn != command.Model.OnProgramme.Ukprn))
        {
            _logger.LogWarning("An unapproved short course episode already exists with another provider for learner with key {LearnerKey}. Cannot create draft.", learner.Key);
            return null;
        }

        //Update existing learning if same provider
        var changes = learning.Update(command.Model);

        await _shortCourseLearningRepository.Update(learning);

        var result = _mapper.Map<CreateDraftShortCourseCommandResult>(learning, learner, command.Model.OnProgramme.Ukprn);
        result.EpisodeKey = learning.LatestEpisode.Key;
        result.IsReinstated = changes.Contains(ShortCourseUpdateChanges.Reinstated);
        return result;
    }

    private ShortCourseLearningDomainModel CreateNewLearning(CreateDraftShortCourseCommand command, LearnerDomainModel learner)
    {
        var learning = _shortCourseLearningFactory.CreateNew(
            learner.Key,
            command.Model.OnProgramme.CompletionDate);

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
            await _learnerRepository.Update(learner);

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

}