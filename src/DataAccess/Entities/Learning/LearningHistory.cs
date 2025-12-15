namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("History.LearningHistory")]
[System.ComponentModel.DataAnnotations.Schema.Table("LearningHistory")]
public class LearningHistory
{
    [Key]
    public Guid Key { get; set; }

    public Guid LearningId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string State { get; set; } = string.Empty;
}