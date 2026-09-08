namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Dapper.Contrib.Extensions.Table("History.ShortCourseLearningHistory")]
public class ShortCourseLearningHistory
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid LearningKey { get; set; }

    public int? AcademicYear { get; set; }

    public string Operation { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public string State { get; set; } = string.Empty;
}
