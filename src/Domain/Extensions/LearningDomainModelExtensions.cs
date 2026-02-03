using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.Extensions;

public static class LearningDomainModelExtensions
{
    public static LearnerUpdatedEvent ToLearnerUpdatedEvent(this LearningDomainModel learner)
    {
        return new LearnerUpdatedEvent
        {
            Key = learner.Key,
            ApprovalsApprenticeshipId = learner.ApprovalsApprenticeshipId,
            Uln = learner.Uln,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            EmailAddress = learner.EmailAddress,
            DateOfBirth = learner.DateOfBirth,
            CompletionDate = learner.CompletionDate,

            Episodes = learner.Episodes.Select(e => new Episode
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
                LastDayOfLearning = e.LastDayOfLearning,
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

            FreezeRequests = learner.FreezeRequests.Select(f => new FreezeRequest
            {
                Key = f.Key,
                FrozenBy = f.FrozenBy,
                FrozenDateTime = f.FrozenDateTime,
                Unfrozen = f.Unfrozen,
                UnfrozenDateTime = f.UnfrozenDateTime,
                UnfrozenBy = f.UnfrozenBy,
                Reason = f.Reason
            }).ToList(),

            MathsAndEnglishCourses = learner.MathsAndEnglishCourses.Select(m => new MathsAndEnglish
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
