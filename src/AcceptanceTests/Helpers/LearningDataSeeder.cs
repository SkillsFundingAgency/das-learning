using AutoFixture;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal class LearningDataSeeder
{
    private ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;

    public LearningDataSeeder(ScenarioContext scenarioContext, TestContext testContext, Fixture fixture)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = fixture;
    }

    internal async Task<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent> CreateLearner(
        DateTime? actualStartDate, DateTime endDate, decimal trainingPrice, decimal epaPrice, DateTime? plannedStartDate = null)
    {
        if (actualStartDate == null && plannedStartDate == null)
            throw new ArgumentException("Either StartDate (ActualStartDate) or PlannedStartDate must be provided");

        var approvalCreatedEvent = _fixture.Build<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>()
            .With(_ => _.ProviderId, Constants.UkPrn)
            .With(_ => _.TrainingCourseVersion, "1.0")
            .With(_ => _.IsOnFlexiPaymentPilot, true)
            .With(_ => _.Uln, _scenarioContext.GetNextUln().ToString)
            .With(_ => _.TrainingCode, _fixture.Create<int>().ToString)
            .With(_ => _.DateOfBirth, actualStartDate?.AddYears(-19) ?? plannedStartDate.GetValueOrDefault().AddYears(-19))
            .With(_ => _.ActualStartDate, actualStartDate)
            .With(_ => _.StartDate, plannedStartDate ?? actualStartDate!.Value)
            .With(_ => _.EndDate, endDate)
            .With(_ => _.FirstName, _fixture.Create<string>())
            .With(_ => _.LastName, _fixture.Create<string>())
            .With(_ => _.PriceEpisodes, new CommitmentsV2.Messages.Events.PriceEpisode[] {
                new CommitmentsV2.Messages.Events.PriceEpisode
                {
                    Cost = trainingPrice + epaPrice,
                    FromDate = actualStartDate ?? plannedStartDate!.Value,
                    ToDate = endDate,
                    EndPointAssessmentPrice = epaPrice,
                    TrainingPrice = trainingPrice
                }
            })
            .Create();

        await _testContext.TestFunction!.PublishEvent(approvalCreatedEvent);

        return approvalCreatedEvent;
    }
}
