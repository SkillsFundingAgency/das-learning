namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.MathsAndEnglishBreakInLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("MathsAndEnglishBreakInLearning")]
public class MathsAndEnglishBreakInLearning
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid MathsAndEnglishKey { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime PriorPeriodExpectedEndDate { get; set; }
}
