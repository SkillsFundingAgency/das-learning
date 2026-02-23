using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;

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

        learning.AddEpisode(
            command.Model.OnProgramme.Ukprn, 
            command.Model.OnProgramme.EmployerId,
            command.Model.OnProgramme.CourseCode,
            false,
            command.Model.OnProgramme.StartDate,
            command.Model.OnProgramme.ExpectedEndDate,
            command.Model.OnProgramme.WithdrawalDate,
            command.Model.OnProgramme.Milestones);

        await _shortCourseLearningRepository.Add(learning);

        //todo we may want to send an event here in a future story but there isn't one on the tech design at present (event on tech design is LearningDataEvent which comes from outer)

        return new CreateDraftShortCourseResult() { LearningKey = learning.Key };
    }

    private async Task<LearnerDomainModel> GetOrCreateLearner(CreateDraftShortCourseCommand command)
    {
        var learner = await _learnerRepository.GetByUln(command.Model.Learner.Uln);

        if (learner != null)
        {
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