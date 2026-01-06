namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Dapper.Contrib.Extensions.Table("History.LearningHistory")]
public class LearningHistory
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid LearningId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string State { get; set; } = string.Empty;
}