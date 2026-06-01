using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.RemoveShortCourse;

public class RemoveShortCourseResult : ShortCourseLearningDto
{
    public Guid RemovedEpisodeKey { get; set; }
}

public class RemoveShortCourseCommand : ICommand
{
    public Guid LearningKey { get; }
    public long Ukprn { get; }

    public RemoveShortCourseCommand(Guid learningKey, long ukprn)
    {
        LearningKey = learningKey;
        Ukprn = ukprn;
    }
}