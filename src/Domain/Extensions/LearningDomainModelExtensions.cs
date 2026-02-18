using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.Extensions;

public static class LearningDomainModelExtensions
{
    //public static LearnerUpdatedEvent ToLearnerUpdatedEvent(this ApprenticeshipLearningDomainModel learning, LearnerModel learnerModel)
    //{
    //    return new LearnerUpdatedEvent
    //    {
    //        Key = learning.Key,
    //        ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
    //        Uln = learnerModel.Uln,
    //        FirstName = learnerModel.FirstName,
    //        LastName = learnerModel.LastName,
    //        EmailAddress = learnerModel.EmailAddress,
    //        DateOfBirth = learnerModel.DateOfBirth,
    //        CompletionDate = learning.CompletionDate,

    //        Episodes = learning.Episodes.Select(e => new Episode
    //        {
    //            Key = e.Key,
    //            Ukprn = e.Ukprn,
    //            EmployerAccountId = e.EmployerAccountId,
    //            FundingType = e.FundingType,
    //            FundingPlatform = e.FundingPlatform,
    //            FundingEmployerAccountId = e.FundingEmployerAccountId,
    //            LegalEntityName = e.LegalEntityName,
    //            AccountLegalEntityId = e.AccountLegalEntityId,
    //            TrainingCode = e.TrainingCode,
    //            TrainingCourseVersion = e.TrainingCourseVersion,
    //            PaymentsFrozen = e.PaymentsFrozen,
    //            WithdrawalDate = e.WithdrawalDate,
    //            PauseDate = e.PauseDate,

    //            LearningSupport = e.LearningSupport.Select(ls => new LearningSupport
    //            {
    //                Key = ls.Key,
    //                LearningKey = ls.LearningKey,
    //                StartDate = ls.StartDate,
    //                EndDate = ls.EndDate
    //            }).ToList(),

    //            EpisodeBreaksInLearning = e.EpisodeBreaksInLearning.Select(b => new EpisodeBreakInLearning
    //            {
    //                Key = b.Key,
    //                StartDate = b.StartDate,
    //                EndDate = b.EndDate,
    //                PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
    //            }).ToList(),

    //            EpisodePrices = e.EpisodePrices.Select(p => new EpisodePrice
    //            {
    //                Key = p.Key,
    //                StartDate = p.StartDate,
    //                EndDate = p.EndDate,
    //                TotalPrice = p.TotalPrice,
    //                TrainingPrice = p.TrainingPrice,
    //                EndPointAssessmentPrice = p.EndPointAssessmentPrice
    //            }).ToList()
    //        }).ToList(),

    //        MathsAndEnglishCourses = learning.MathsAndEnglishCourses.Select(m => new MathsAndEnglish
    //        {
    //            Key = m.Key,
    //            StartDate = m.StartDate,
    //            PlannedEndDate = m.PlannedEndDate,
    //            Course = m.Course,
    //            WithdrawalDate = m.WithdrawalDate,
    //            CompletionDate = m.CompletionDate,
    //            PauseDate = m.PauseDate,
    //            PriorLearningPercentage = m.PriorLearningPercentage,
    //            Amount = m.Amount
    //        }).ToList()
    //    };
    //}

}
