using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.Episode")]
[System.ComponentModel.DataAnnotations.Schema.Table("Episode")]
public class Episode
{
    public Episode()
    {
        Prices = new List<EpisodePrice>();
    }

    [Key]
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public FundingType FundingType { get; set; }
    public FundingPlatform? FundingPlatform { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string TrainingCode { get; set; } = null!;
    public string? TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public List<EpisodePrice> Prices { get; set; }
    public string LearningStatus { get; set; }
    public DateTime? LastDayOfLearning { get; set; }
}