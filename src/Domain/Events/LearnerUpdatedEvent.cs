using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Events;

public class LearnerUpdatedEvent : IDomainEvent
{
    public static LearnerUpdatedEvent From(LearnerDomainModel learner, ApprenticeshipLearningDomainModel learning)
    {
        return new LearnerUpdatedEvent
        {
            Key = learning.Key,
            ApprovalsApprenticeshipId = learning.LatestEpisode.ApprovalsApprenticeshipId,
            Uln = learner.Uln,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            EmailAddress = learner.EmailAddress,
            DateOfBirth = learner.DateOfBirth,
            CompletionDate = learning.CompletionDate,

            Episodes = learning.Episodes.Select(e => new Episode
            {
                Key = e.Key,
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                FundingType = e.FundingType,
                FundingPlatform = e.FundingPlatform,
                FundingEmployerAccountId = e.FundingEmployerAccountId,
                LegalEntityName = e.LegalEntityName,
                AccountLegalEntityId = e.AccountLegalEntityId,
                TrainingCode = e.TrainingCode,
                TrainingCourseVersion = e.TrainingCourseVersion,
                PaymentsFrozen = e.PaymentsFrozen,
                WithdrawalDate = e.WithdrawalDate,
                PauseDate = e.PauseDate,

                LearningSupport = e.LearningSupport.Select(ls => new LearningSupport
                {
                    Key = ls.Key,
                    LearningKey = ls.LearningKey,
                    StartDate = ls.StartDate,
                    EndDate = ls.EndDate
                }).ToList(),

                EpisodeBreaksInLearning = e.EpisodeBreaksInLearning.Select(b => new EpisodeBreakInLearning
                {
                    Key = b.Key,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
                }).ToList(),

                EpisodePrices = e.EpisodePrices.Select(p => new EpisodePrice
                {
                    Key = p.Key,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalPrice = p.TotalPrice,
                    TrainingPrice = p.TrainingPrice,
                    EndPointAssessmentPrice = p.EndPointAssessmentPrice
                }).ToList()
            }).ToList(),

            EnglishAndMathsCourses = learning.EnglishAndMathsCourses.Select(m => new EnglishAndMaths
            {
                Key = m.Key,
                StartDate = m.StartDate,
                PlannedEndDate = m.PlannedEndDate,
                Course = m.Course,
                WithdrawalDate = m.WithdrawalDate,
                CompletionDate = m.CompletionDate,
                PauseDate = m.PauseDate,
                PriorLearningPercentage = m.PriorLearningPercentage,
                Amount = m.Amount
            }).ToList()
        };
    }


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