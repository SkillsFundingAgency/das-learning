using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(long ukprn, int academicYear, List<ShortCourseUpdateContext> models)
    {
        Ukprn = ukprn;
        AcademicYear = academicYear;
        Models = models;
    }

    public long Ukprn { get; }
    public int AcademicYear { get; }
    public List<ShortCourseUpdateContext> Models { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid? LearningKey { get; set; }
    public Guid? EpisodeKey { get; set; }
}
