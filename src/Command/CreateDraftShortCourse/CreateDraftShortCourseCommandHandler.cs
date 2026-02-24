using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandHandler : ICommandHandler<CreateDraftShortCourseCommand, CreateDraftShortCourseResult>
{
    private readonly ILearnerFactory _learnerFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IShortCourseLearningRepository _shortCourseLearningRepository;
    private readonly IShortCourseLearningFactory _shortCourseLearningFactory;
    private readonly ILogger<CreateDraftShortCourseCommandHandler> _logger;

    public CreateDraftShortCourseCommandHandler(
        ILearnerFactory learnerFactory,
        ILearnerRepository learnerRepository,
        IShortCourseLearningRepository shortCourseLearningRepository,
        IShortCourseLearningFactory shortCourseLearningFactory,
        ILogger<CreateDraftShortCourseCommandHandler> logger)
    {
        _learnerFactory = learnerFactory;
        _learnerRepository = learnerRepository;
        _shortCourseLearningRepository = shortCourseLearningRepository;
        _shortCourseLearningFactory = shortCourseLearningFactory;
        _logger = logger;
    }

    public async Task<CreateDraftShortCourseResult> Handle(CreateDraftShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftShortCourseCommand");

        var learner = await GetOrCreateLearner(command);

        var learning = _shortCourseLearningFactory.CreateNew(
            learner.Key,
            command.Model.OnProgramme.CompletionDate);

        var episode = learning.AddEpisode(
            command.Model.OnProgramme.Ukprn, 
            command.Model.OnProgramme.EmployerId,
            command.Model.OnProgramme.CourseCode,
            false,
            command.Model.OnProgramme.StartDate,
            command.Model.OnProgramme.ExpectedEndDate,
            command.Model.OnProgramme.WithdrawalDate,
            command.Model.OnProgramme.Milestones);

        foreach (var learningSupport in command.Model.LearningSupport)
        {
            episode.AddLearningSupport(learningSupport.StartDate, learningSupport.EndDate);
        }

        TransferEvents(learner, learning);
        await _shortCourseLearningRepository.Add(learning);

        //todo we may want to send an event here in a future story but there isn't one on the tech design at present (event on tech design is LearningDataEvent which comes from outer)

        return new CreateDraftShortCourseResult() { LearningKey = learning.Key };
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