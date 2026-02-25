using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(CreateDraftShortCourseModel model)
    {
        Model = model;
    }

    public CreateDraftShortCourseModel Model { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid LearningKey { get; set; }
}