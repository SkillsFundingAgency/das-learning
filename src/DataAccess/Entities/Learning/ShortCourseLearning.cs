namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ShortCourseLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseLearning")]
public class ShortCourseLearning : Learning
{
    public ShortCourseLearning()
    {
        Episodes = new List<ShortCourseEpisode>();
    }

    public List<ShortCourseEpisode> Episodes { get; set; }
}