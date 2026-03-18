namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

#pragma warning disable CS8618

[Table("dbo.ShortCourseEpisode")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseEpisode")]
public class ShortCourseEpisode : Episode
{
    public ShortCourseEpisode()
    {
        Milestones = new List<ShortCourseMilestone>();
        LearningSupport = new List<ShortCourseLearningSupport>();
    }

    public DateTime? WithdrawalDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public bool IsApproved { get; set; }
    public decimal Price { get; set; }
    public string LearnerRef { get; set; }

    public List<ShortCourseMilestone> Milestones { get; set; }
    public List<ShortCourseLearningSupport> LearningSupport { get; set; }
}
#pragma warning restore CS8618