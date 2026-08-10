using SFA.DAS.Learning.Command.Shared;

namespace SFA.DAS.Learning.Command.RemoveShortCourse;

public class RemoveShortCourseResult
{
    public List<RemoveShortCourseItemResult> Results { get; set; } = [];
}

public class RemoveShortCourseItemResult : ShortCourseCommandResult
{
    public Guid RemovedEpisodeKey { get; set; }
}

public class RemoveShortCourseCommand : ICommand
{
    public Guid LearnerKey { get; }
    public long Ukprn { get; }
    public int AcademicYear { get; }

    public RemoveShortCourseCommand(Guid learnerKey, long ukprn, int academicYear)
    {
        LearnerKey = learnerKey;
        Ukprn = ukprn;
        AcademicYear = academicYear;
    }
}