namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.Learning")]
[System.ComponentModel.DataAnnotations.Schema.Table("Learning")]
public class Learning
{
	public Learning()
	{
		FreezeRequests = new List<FreezeRequest>();
        Episodes = new List<Episode>();
        MathsAndEnglishCourses = new List<MathsAndEnglish>();
    }
        
	[Key]
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
}