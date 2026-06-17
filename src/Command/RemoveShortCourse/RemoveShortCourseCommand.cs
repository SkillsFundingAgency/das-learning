using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.RemoveShortCourse;

public class RemoveShortCourseResult
{
    public List<RemoveShortCourseItemResult> Results { get; set; } = [];
}

public class RemoveShortCourseItemResult : ShortCourseLearningDto
{
    public Guid RemovedEpisodeKey { get; set; }
}

public class RemoveShortCourseCommand : ICommand
{
    public Guid LearnerKey { get; }
    public long Ukprn { get; }

    public RemoveShortCourseCommand(Guid learnerKey, long ukprn)
    {
        LearnerKey = learnerKey;
        Ukprn = ukprn;
    }
}