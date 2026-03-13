using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Events;

public class LearnerUpdatedEvent : IDomainEvent
{
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<Episode> Episodes { get; set; }
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<EnglishAndMaths> EnglishAndMathsCourses { get; set; }
}

public class Episode
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public FundingType FundingType { get; set; }
    public FundingPlatform? FundingPlatform { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string TrainingCode { get; set; }
    public string TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public List<LearningSupport> LearningSupport { get; set; }
    public List<EpisodeBreakInLearning> EpisodeBreaksInLearning { get; set; }
    public List<EpisodePrice> EpisodePrices { get; set; }
}

public class LearningSupport
{
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class EpisodeBreakInLearning
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}

public class EpisodePrice
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? EndPointAssessmentPrice { get; set; }
    public decimal? TrainingPrice { get; set; }
}

public class FreezeRequest
{
    public Guid Key { get; set; }
    public string FrozenBy { get; set; }
    public DateTime FrozenDateTime { get; set; }
    public bool Unfrozen { get; set; }
    public DateTime? UnfrozenDateTime { get; set; }
    public string? UnfrozenBy { get; set; }
    public string? Reason { get; set; }
}

public class EnglishAndMaths
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Course { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public int? PriorLearningPercentage { get; set; }
    public decimal Amount { get; set; }
}