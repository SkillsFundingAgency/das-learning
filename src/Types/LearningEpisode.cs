namespace SFA.DAS.Learning.Types;

#pragma warning disable CS8618
public class LearningEpisode
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
	public Enums.FundingType FundingType { get; set; }
	public Enums.FundingPlatform? FundingPlatform { get; set; }
	public long? FundingEmployerAccountId { get; set; }
	public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public int AgeAtStartOfLearning { get; set; }
    public string TrainingCode { get; set; } = null!;
    public string? TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public List<LearningEpisodePrice> Prices { get; set; }
}
#pragma warning restore CS8618