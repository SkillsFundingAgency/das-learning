using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.Domain.Extensions;

public static class IntegrationEventExtensions
{
    public static LearningEpisode BuildEpisodeForIntegrationEvent(this LearningDomainModel learning)
    {
        var latestEpisode = learning.LatestEpisode;
        var activePrices = latestEpisode.ActiveEpisodePrices;
        var prices = activePrices.Select(price => 
            new LearningEpisodePrice
            {
                Key = price.Key,
                StartDate = price.StartDate,
                EndDate = price.EndDate,
                TrainingPrice = price.TrainingPrice,
                EndPointAssessmentPrice = price.EndPointAssessmentPrice,
                TotalPrice = price.TotalPrice,
                FundingBandMaximum = price.FundingBandMaximum
            }).ToList();

        return new LearningEpisode()
        {
            Key = latestEpisode.Key,
            Ukprn = latestEpisode.Ukprn,
            EmployerAccountId = latestEpisode.EmployerAccountId,
            FundingType = latestEpisode.FundingType,
            FundingPlatform = latestEpisode.FundingPlatform,
            FundingEmployerAccountId = latestEpisode.FundingEmployerAccountId,
            LegalEntityName = latestEpisode.LegalEntityName,
            AccountLegalEntityId = latestEpisode.AccountLegalEntityId,
            AgeAtStartOfLearning = learning.AgeAtStartOfLearning,
            TrainingCode = latestEpisode.TrainingCode,
            TrainingCourseVersion = latestEpisode.TrainingCourseVersion,
            PaymentsFrozen = latestEpisode.PaymentsFrozen,
            Prices = prices
        };
    }
}
