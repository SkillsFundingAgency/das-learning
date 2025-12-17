namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

public class LearningHistory
{
    [Key]
    public Guid Key { get; set; }

    public Guid LearningId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string State { get; set; } = string.Empty;
}