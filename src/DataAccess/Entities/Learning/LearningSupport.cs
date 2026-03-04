namespace SFA.DAS.Learning.DataAccess.Entities.Learning;


[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public abstract class LearningSupport
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid LearningKey { get; set; }

    public Guid EpisodeKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}

[Table("dbo.ApprenticeshipLearningSupport")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipLearningSupport")]
public class ApprenticeshipLearningSupport : LearningSupport
{

}

[Table("dbo.ShortCourseLearningSupport")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseLearningSupport")]
public class ShortCourseLearningSupport : LearningSupport
{

}