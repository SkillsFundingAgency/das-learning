namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.MathsAndEnglish")]
[System.ComponentModel.DataAnnotations.Schema.Table("MathsAndEnglish")]
public class MathsAndEnglish
{
    [Key]
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Course { get; set; } = null!;
    public DateTime? WithdrawalDate { get; set; }
    public int? PriorLearningPercentage { get; set; }
    public decimal Amount { get; set; }
}