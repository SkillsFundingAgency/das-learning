using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandHandler : ICommandHandler<CreateDraftShortCourseCommand, CreateDraftShortCourseResult>
{
    private readonly IShortCourseLearningRepository _learningRepository;
    private readonly IShortCourseLearningFactory _learningFactory;
    private readonly ILogger<CreateDraftShortCourseCommandHandler> _logger;

    public CreateDraftShortCourseCommandHandler(
        IShortCourseLearningRepository learningRepository,
        IShortCourseLearningFactory learningFactory,
        ILogger<CreateDraftShortCourseCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _learningFactory = learningFactory;
        _logger = logger;
    }

    public async Task<CreateDraftShortCourseResult> Handle(CreateDraftShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftShortCourseCommand");

        var learning = _learningFactory.CreateNew(
            command.Model.OnProgramme.WithdrawalDate,
            command.Model.OnProgramme.ExpectedEndDate,
            command.Model.OnProgramme.CompletionDate,
            false);

        learning.AddEpisode(
            command.Model.OnProgramme.Ukprn, 
            command.Model.OnProgramme.EmployerId, 
            command.Model.OnProgramme.CourseCode, 
            command.Model.OnProgramme.Milestones);

        await _learningRepository.Add(learning);

        //todo we may want to send an event here in a future story but there isn't one on the tech design at present (event on tech design is LearningDataEvent which comes from outer)

        return new CreateDraftShortCourseResult() { LearningKey = learning.Key };
    }
}