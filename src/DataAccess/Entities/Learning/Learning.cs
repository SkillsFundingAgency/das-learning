namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.Learning")]
[System.ComponentModel.DataAnnotations.Schema.Table("Learning")]
public class Learning
{
	public Learning()
	{
		PriceHistories = new List<PriceHistory>();
		StartDateChanges = new List<StartDateChange>();
		FreezeRequests = new List<FreezeRequest>();
        Episodes = new List<Episode>();
        WithdrawalRequests = new List<WithdrawalRequest>();
    }
        
	[Key]
	public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public DateTime DateOfBirth { get; set; }
	public string ApprenticeshipHashedId { get; set; } = null!;
    public List<PriceHistory> PriceHistories { get; set; }
    public List<StartDateChange> StartDateChanges { get; set; }
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<Episode> Episodes { get; set; }
    public List<WithdrawalRequest> WithdrawalRequests { get; set; }
    public DateTime? CompletionDate { get; set; }
}