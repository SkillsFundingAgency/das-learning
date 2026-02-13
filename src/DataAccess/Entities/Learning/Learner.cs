namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

#pragma warning disable CS8618

[Table("dbo.Learner")]
[System.ComponentModel.DataAnnotations.Schema.Table("Learner")]
public class Learner
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public string Uln { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }

}

#pragma warning restore CS8618