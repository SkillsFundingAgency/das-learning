using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(List<ShortCourseUpdateContext> models)
    {
        Models = models;
    }

    public List<ShortCourseUpdateContext> Models { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid? LearningKey { get; set; }
    public Guid? EpisodeKey { get; set; }
}
