using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ApprenticeshipEpisode")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipEpisode")]
public class ApprenticeshipEpisode : Episode
{
    public ApprenticeshipEpisode()
    {
        Prices = new List<EpisodePrice>();
        LearningSupport = new List<ApprenticeshipLearningSupport>();
        BreaksInLearning = new List<EpisodeBreakInLearning>();
    }

    public long ApprovalsApprenticeshipId { get; set; }
    public FundingType FundingType { get; set; }
    public FundingPlatform? FundingPlatform { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string? TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public List<EpisodePrice> Prices { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public List<ApprenticeshipLearningSupport> LearningSupport { get; set; }
    public List<EpisodeBreakInLearning> BreaksInLearning { get; set; }
}