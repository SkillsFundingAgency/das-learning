using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Events;

#pragma warning disable CS8618 // Required properties must be set in the constructor

public class ApprenticeshipLearningChangedEvent : IDomainEvent
{
    public static ApprenticeshipLearningChangedEvent From(
        ApprenticeshipLearningDomainModel learning,
        int? academicYear,
        ApprenticeshipLearningOperation operation,
        IReadOnlyCollection<LearningUpdateChanges>? changes = null)
    {
        return new ApprenticeshipLearningChangedEvent
        {
            LearningKey = learning.Key,
            AcademicYear = academicYear,
            Operation = operation,
            Changes = changes?.ToList() ?? [],
            Snapshot = ApprenticeshipLearningSnapshot.From(learning)
        };
    }

    public Guid LearningKey { get; set; }
    public int? AcademicYear { get; set; }
    public ApprenticeshipLearningOperation Operation { get; set; }
    public List<LearningUpdateChanges> Changes { get; set; }
    public ApprenticeshipLearningSnapshot Snapshot { get; set; }
}

public class ApprenticeshipLearningSnapshot
{
    public static ApprenticeshipLearningSnapshot From(ApprenticeshipLearningDomainModel learning)
    {
        return new ApprenticeshipLearningSnapshot
        {
            LearningKey = learning.Key,
            LearnerKey = learning.LearnerKey,
            CompletionDate = learning.CompletionDate,
            AchievementDate = learning.AchievementDate,
            LearningType = learning.LearningType,
            TrainingCode = learning.TrainingCode,
            TrainingCourseVersion = learning.TrainingCourseVersion,

            Episodes = learning.Episodes.Select(e => new ApprenticeshipEpisodeSnapshot
            {
                Key = e.Key,
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                FundingEmployerAccountId = e.FundingEmployerAccountId,
                LegalEntityName = e.LegalEntityName,
                AccountLegalEntityId = e.AccountLegalEntityId,
                IsApproved = e.IsApproved,
                PaymentsFrozen = e.PaymentsFrozen,
                IsRemoved = e.IsRemoved,
                WithdrawalDate = e.WithdrawalDate,
                PauseDate = e.PauseDate,
                EmployerType = e.EmployerType,
                ApprovalsApprenticeshipId = e.ApprovalsApprenticeshipId,

                LearningSupport = e.LearningSupport.Select(ls => new ApprenticeshipLearningSupportSnapshot
                {
                    Key = ls.Key,
                    StartDate = ls.StartDate,
                    EndDate = ls.EndDate
                }).ToList(),

                EpisodeBreaksInLearning = e.EpisodeBreaksInLearning.Select(b => new EpisodeBreakInLearningSnapshot
                {
                    Key = b.Key,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
                }).ToList(),

                EpisodePrices = e.EpisodePrices.Select(p => new EpisodePriceSnapshot
                {
                    Key = p.Key,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalPrice = p.TotalPrice,
                    TrainingPrice = p.TrainingPrice,
                    EndPointAssessmentPrice = p.EndPointAssessmentPrice
                }).ToList()
            }).ToList(),

            EnglishAndMathsCourses = learning.EnglishAndMathsCourses.Select(m => new EnglishAndMathsSnapshot
            {
                Key = m.Key,
                StartDate = m.StartDate,
                PlannedEndDate = m.PlannedEndDate,
                Course = m.Course,
                WithdrawalDate = m.WithdrawalDate,
                CompletionDate = m.CompletionDate,
                PauseDate = m.PauseDate,
                CombinedFundingAdjustmentPercentage = m.CombinedFundingAdjustmentPercentage,
                Amount = m.Amount
            }).ToList()
        };
    }

    public Guid LearningKey { get; set; }
    public Guid LearnerKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? AchievementDate { get; set; }
    public LearningType LearningType { get; set; }
    public string TrainingCode { get; set; }
    public string? TrainingCourseVersion { get; set; }
    public List<ApprenticeshipEpisodeSnapshot> Episodes { get; set; }
    public List<EnglishAndMathsSnapshot> EnglishAndMathsCourses { get; set; }
}

public class ApprenticeshipEpisodeSnapshot
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long? EmployerAccountId { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public bool IsApproved { get; set; }
    public bool PaymentsFrozen { get; set; }
    public bool IsRemoved { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public EmployerType EmployerType { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public List<ApprenticeshipLearningSupportSnapshot> LearningSupport { get; set; }
    public List<EpisodeBreakInLearningSnapshot> EpisodeBreaksInLearning { get; set; }
    public List<EpisodePriceSnapshot> EpisodePrices { get; set; }
}

public class ApprenticeshipLearningSupportSnapshot
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class EpisodeBreakInLearningSnapshot
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}

public class EpisodePriceSnapshot
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? EndPointAssessmentPrice { get; set; }
    public decimal? TrainingPrice { get; set; }
}

public class EnglishAndMathsSnapshot
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Course { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public decimal? CombinedFundingAdjustmentPercentage { get; set; }
    public decimal Amount { get; set; }
}

#pragma warning restore CS8618 // Required properties must be set in the constructor
