using System;
using System.Linq;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.UnitTests.Helpers;

public static class LearningUpdateModelHelper
{
    public static LearningUpdateContext CreateUpdateModel(
        DataAccess.Entities.Learning.ApprenticeshipLearning learning,
        DataAccess.Entities.Learning.Learner learner)
    {
        return new LearningUpdateContext
        {
            LearningKey = learning.Key,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            Learner = new LearnerModel
            {
                Uln = learner.Uln,
                FirstName = learner.FirstName,
                LastName = learner.LastName,
                DateOfBirth = learner.DateOfBirth,
                EmailAddress = learner.EmailAddress
            },
            Care = new CareDetails
            {
                HasEHCP = learner.HasEHCP,
                IsCareLeaver = learner.IsCareLeaver,
                CareLeaverEmployerConsentGiven = learner.CareLeaverEmployerConsentGiven
            },
            Learning = new LearningUpdateDetails
            {
                CompletionDate = learning.CompletionDate
            },
            EnglishAndMathsCourses = learning.EnglishAndMathsCourses.Select(x => new EnglishAndMathsUpdateDetails
            {
                Course = x.Course,
                LearnAimRef = x.LearnAimRef,
                StartDate = x.StartDate,
                PlannedEndDate = x.PlannedEndDate,
                CompletionDate = x.CompletionDate,
                WithdrawalDate = x.WithdrawalDate,
                PauseDate = x.PauseDate,
                PriorLearningPercentage = x.PriorLearningPercentage,
                Amount = x.Amount,
                BreaksInLearning = x.BreaksInLearning.Select(b => new BreakInLearningUpdateDetails
                {
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
                }).ToList()
            }).ToList(),
            LearningSupport = learning.GetEpisode().LearningSupport.Select(x => new LearningSupportDetails
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToList(),
            OnProgrammeDetails = new OnProgrammeDetails
            {
                ExpectedEndDate = learning.GetEpisode().Prices.MaxBy(p => p.StartDate).EndDate,
                Costs = learning.GetEpisode().Prices.Select(x => new Cost
                {
                    FromDate = x.StartDate,
                    TrainingPrice = Convert.ToInt32(x.TrainingPrice.Value),
                    EpaoPrice = Convert.ToInt32(x.EndPointAssessmentPrice.Value)
                }).ToList(),
                PauseDate = learning.GetEpisode().PauseDate,
                BreaksInLearning = learning.GetEpisode().BreaksInLearning.Select(b => new BreakInLearningUpdateDetails
                {
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
                }).ToList()
            },
            Delivery = new DeliveryDetails
            {
                WithdrawalDate = learning.GetEpisode().WithdrawalDate
            }
        };
    }
}