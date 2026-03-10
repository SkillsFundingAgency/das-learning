namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.EnglishAndMaths")]
[System.ComponentModel.DataAnnotations.Schema.Table("EnglishAndMaths")]
public class EnglishAndMaths
{
    public EnglishAndMaths()
    {
        BreaksInLearning = new List<EnglishAndMathsBreakInLearning>();
    }

    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Course { get; set; } = null!;
    public string LearnAimRef { get; set; } = null!;
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public int? PriorLearningPercentage { get; set; }
    public decimal Amount { get; set; }
    public List<EnglishAndMathsBreakInLearning> BreaksInLearning { get; set; }
}
