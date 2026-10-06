using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class ApprenticeshipCreatedEventExtensions
{
    internal static UpdateLearnerRequest BuildUpdateLearnerRequest(this CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var priceEpisode = apprenticeshipCreatedEvent.PriceEpisodes.Single();

        var request = new UpdateLearnerRequest
        {
            Delivery = new Delivery(),
            Learner = new ApprenticeshipLearnerUpdateDetails
            {
                FirstName = apprenticeshipCreatedEvent.FirstName,
                LastName = apprenticeshipCreatedEvent.LastName,
                EmailAddress = null,
                CompletionDate = null,
                DateOfBirth = apprenticeshipCreatedEvent.DateOfBirth,
                Care = new CareDetails(),
                Uln = long.Parse(apprenticeshipCreatedEvent.Uln)
            },
            EnglishAndMathsCourses = new List<EnglishAndMaths>(),
            OnProgramme = new OnProgrammeDetails
            {
                WithdrawalDate = null,
                TrainingCode = apprenticeshipCreatedEvent.TrainingCode,
                LearningSupport = new List<LearningSupportDetails>(),
                Costs = new List<Cost>
                {
                    new Cost
                    {
                        EpaoPrice = (int?)priceEpisode.EndPointAssessmentPrice,
                        FromDate = apprenticeshipCreatedEvent.ActualStartDate!.Value,
                        TrainingPrice = (int)priceEpisode.TrainingPrice!
                    }
                },
                ExpectedEndDate = apprenticeshipCreatedEvent.EndDate,
                BreaksInLearning = new List<BreakInLearning>()
            }
        };

        return request;
    }
}
