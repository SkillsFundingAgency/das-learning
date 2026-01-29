namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ApprenticeshipLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipLearning")]
public class ApprenticeshipLearning : Learning
{
	public ApprenticeshipLearning()
	{
		FreezeRequests = new List<FreezeRequest>();
        Episodes = new List<ApprenticeshipEpisode>();
        MathsAndEnglishCourses = new List<MathsAndEnglish>();
    }
    public long ApprovalsApprenticeshipId { get; set; }
	public string Uln { get; set; } = null!;
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
    public string? EmailAddress { get; set; }
	public DateTime DateOfBirth { get; set; }
	public string ApprenticeshipHashedId { get; set; } = null!;
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<ApprenticeshipEpisode> Episodes { get; set; }
    public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; }
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }
}

public abstract class Learning
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public DateTime? CompletionDate { get; set; }
}

[Table("dbo.ApprenticeshipUnitLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipUnitLearning")]
public class ApprenticeshipUnitLearning : Learning
{
    public Guid LearnerKey { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public bool IsApproved { get; set; }
}