namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public abstract class Learning
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public DateTime? CompletionDate { get; set; }
}