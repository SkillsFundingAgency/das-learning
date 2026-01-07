namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.FreezeRequest")]
[System.ComponentModel.DataAnnotations.Schema.Table("FreezeRequest")]
public class FreezeRequest
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public string FrozenBy { get; set; } = null!;
    public DateTime FrozenDateTime { get; set; }
    public bool Unfrozen { get; set; }
    public DateTime? UnfrozenDateTime { get; set; }
    public string? UnfrozenBy { get; set; }
    public string? Reason { get; set; }
}
