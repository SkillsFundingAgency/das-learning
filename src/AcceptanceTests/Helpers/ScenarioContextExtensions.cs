using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class ScenarioContextExtensions
{
    internal static string GetDbConnectionString(this ScenarioContext context) => context.Get<string>("DbConnectionString");
    internal static void SetDbConnectionString(this ScenarioContext context, string connectionString) => context.Set(connectionString, "DbConnectionString");

    internal static Guid GetLearningKey(this ScenarioContext context) => context.Get<Guid>("LearningKey");
    internal static void SetLearningKey(this ScenarioContext context, Guid learningKey) => context.Set(learningKey, "LearningKey");

    internal static UpdateLearnerRequest GetUpdateLearnerRequest(this ScenarioContext context)
    {
        if (context.ContainsKey(typeof(UpdateLearnerRequest).ToString()))
        {
            return context.Get<UpdateLearnerRequest>();
        }

        CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent;

        try
        {
            apprenticeshipCreatedEvent = context.GetApprenticeshipCreatedEvent();
        }
        catch (Exception ex)
        {
            throw new KeyNotFoundException("Cannot start populating UpdateLearnerRequest until a Learner is created",ex);
        }

        var request = CreateUpdateLearnerRequestFromApprenticeshipCreatedEvent(apprenticeshipCreatedEvent);

        context.SetUpdateLearnerRequest(request);
        return request;
    }

    internal static void SetUpdateLearnerRequest(this ScenarioContext context, UpdateLearnerRequest request) => context.Set(request);

    internal static CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent GetApprenticeshipCreatedEvent(this ScenarioContext context) => context.Get<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>();
    internal static void SetApprenticeshipCreatedEvent(this ScenarioContext context, CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent @event) => context.Set(@event);

    internal static UpdateLearnerResult GetUpdateLearnerResult(this ScenarioContext context) => context.Get<UpdateLearnerResult>();
    internal static void SetUpdateLearnerResult(this ScenarioContext context, UpdateLearnerResult updateLearnerResult) => context.Set(updateLearnerResult);

    private static UpdateLearnerRequest CreateUpdateLearnerRequestFromApprenticeshipCreatedEvent(CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
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
