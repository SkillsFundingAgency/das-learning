using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(ShortCourseUpdateContext model)
    {
        Model = model;
    }

    public ShortCourseUpdateContext Model { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid? LearningKey { get; set; }
    public Guid? EpisodeKey { get; set; }
    public CreateDraftShortCourseResultTypes ResultType { get; set; }
}

public enum CreateDraftShortCourseResultTypes
{
    Success,
    ApprovedAlreadyExists
}