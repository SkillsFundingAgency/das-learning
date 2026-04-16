namespace SFA.DAS.Learning.Command.DeleteShortCourse;

public class DeleteShortCourseResult : ShortCourseLearningResult
{
    public bool WasDeleted { get; set; }
}

public class DeleteShortCourseCommand : ICommand
{
    public Guid LearningKey { get; }
    public long Ukprn { get; }

    public DeleteShortCourseCommand(Guid learningKey, long ukprn)
    {
        LearningKey = learningKey;
        Ukprn = ukprn;
    }
}
