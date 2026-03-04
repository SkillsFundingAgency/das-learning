namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public abstract class Episode
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid LearningKey { get; set; }

    public long Ukprn { get; set; }

    public long EmployerAccountId { get; set; }

    public string TrainingCode { get; set; } = null!;
}