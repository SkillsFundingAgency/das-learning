using SFA.DAS.Learning.InnerApi.Requests;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class ApprenticeshipCreatedEventExtensions
{
    internal static UpdateLearnerRequest BuildUpdateLearnerRequest(this CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var priceEpisode = apprenticeshipCreatedEvent.PriceEpisodes.Single();

        var request = new UpdateLearnerRequest
        {
            Delivery = new Delivery
            {
                WithdrawalDate = null
            },
            Learner = new LearnerUpdateDetails
            {
                FirstName = apprenticeshipCreatedEvent.FirstName,
                LastName = apprenticeshipCreatedEvent.LastName,
                EmailAddress = null,
                CompletionDate = null
            },
            LearningSupport = new List<LearningSupportUpdatedDetails>(),
            MathsAndEnglishCourses = new List<MathsAndEnglish>(),
            OnProgramme = new OnProgrammeDetails
            {
                Costs = new List<Cost>
                {
                    new Cost
                    {
                        EpaoPrice = (int?)priceEpisode.EndPointAssessmentPrice,
                        FromDate = apprenticeshipCreatedEvent.ActualStartDate!.Value,
                        TrainingPrice = (int)priceEpisode.TrainingPrice!
                    }
                },
                ExpectedEndDate = apprenticeshipCreatedEvent.EndDate
            }
        };

        return request;
    }
}
