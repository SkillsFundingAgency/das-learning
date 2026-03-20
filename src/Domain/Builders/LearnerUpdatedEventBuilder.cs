using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.Builders;

public class LearnerUpdatedEventBuilder
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    public LearnerUpdatedEventBuilder(LearnerDomainModel learner, ApprenticeshipLearningDomainModel learning)
    {
        _learner = learner;
        _learning = learning;
    }

    public LearnerUpdatedEvent CreateEvent()
    {
        return new LearnerUpdatedEvent
        {
            Key = _learning.Key,
            ApprovalsApprenticeshipId = _learning.ApprovalsApprenticeshipId,
            Uln = _learner.Uln,
            FirstName = _learner.FirstName,
            LastName = _learner.LastName,
            EmailAddress = _learner.EmailAddress,
            DateOfBirth = _learner.DateOfBirth,
            CompletionDate = _learning.CompletionDate,

            Episodes = _learning.Episodes.Select(e => new Episode
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

            EnglishAndMathsCourses = _learning.EnglishAndMathsCourses.Select(m => new EnglishAndMaths
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

}
