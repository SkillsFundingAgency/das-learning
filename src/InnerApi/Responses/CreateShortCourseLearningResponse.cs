using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.InnerApi.Responses;

#pragma warning disable CS8618
public class CreateShortCourseLearningResponse
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public bool IsReinstated { get; set; }

    // Populated only when IsReinstated = true
    public Guid LearnerKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseLearnerDto? Learner { get; set; }
    public ShortCourseEpisodeDto[] Episodes { get; set; } = [];
}
#pragma warning restore CS8618