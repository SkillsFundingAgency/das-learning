namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.LearningSupport")]
[System.ComponentModel.DataAnnotations.Schema.Table("LearningSupport")]
public class LearningSupport
{
    [Key]
    public Guid Key { get; set; }

    public Guid LearningKey { get; set; }

    public Guid EpisodeKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
