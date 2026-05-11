using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandResult
{
    public Guid? LearningKey { get; set; }
    public Guid? EpisodeKey { get; set; }
    public bool IsReinstated { get; set; }
    public Guid LearnerKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseLearnerDto? Learner { get; set; }
    public ShortCourseEpisodeDto[] Episodes { get; set; } = [];
}