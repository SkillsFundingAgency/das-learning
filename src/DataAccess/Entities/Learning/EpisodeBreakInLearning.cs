namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.EpisodeBreakInLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("EpisodeBreakInLearning")]
public class EpisodeBreakInLearning
{
    [Key]
    public Guid Key { get; set; }

    public Guid EpisodeKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}