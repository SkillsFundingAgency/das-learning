namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ShortCourseEpisode")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseEpisode")]
public class ShortCourseEpisode : Episode
{
    public ShortCourseEpisode()
    {
        Milestones = new List<ShortCourseMilestone>();
    }

    public List<ShortCourseMilestone> Milestones { get; set; }
}