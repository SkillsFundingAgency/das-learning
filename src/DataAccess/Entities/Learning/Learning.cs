namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ApprenticeshipLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipLearning")]
public class ApprenticeshipLearning
{
	public ApprenticeshipLearning()
	{
		FreezeRequests = new List<FreezeRequest>();
        Episodes = new List<Episode>();
        MathsAndEnglishCourses = new List<MathsAndEnglish>();
    }
        
	[Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
    public string? EmailAddress { get; set; }
	public DateTime DateOfBirth { get; set; }
	public string ApprenticeshipHashedId { get; set; } = null!;
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<Episode> Episodes { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; }
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }
}