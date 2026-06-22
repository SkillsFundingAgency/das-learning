using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(long ukprn, List<ShortCourseUpdateContext> models)
    {
        Ukprn = ukprn;
        Models = models;
    }

    public long Ukprn { get; }
    public List<ShortCourseUpdateContext> Models { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid? LearningKey { get; set; }
    public Guid? EpisodeKey { get; set; }
}
