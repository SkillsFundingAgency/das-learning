using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.DeleteShortCourse;

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

public class DeleteShortCourseResult
{
    public Guid LearningKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseLearner Learner { get; set; } = null!;
    public ShortCourseEpisode[] Episodes { get; set; } = [];
}