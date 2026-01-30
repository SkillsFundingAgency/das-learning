namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ShortCourseLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseLearning")]
public class ShortCourseLearning : Learning
{
    public ShortCourseLearning()
    {
        Episodes = new List<ShortCourseEpisode>();
    }

    public Guid LearnerKey { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public bool IsApproved { get; set; }
    public List<ShortCourseEpisode> Episodes { get; set; }
}