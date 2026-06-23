using Dapper;
using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class FreezeLearningStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public FreezeLearningStepDefinitions(
        ScenarioContext scenarioContext,
        TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When(@"Approvals inform us that the employer has (.*) a short course")]
    public async Task WhenApprovalsInformUsThatTheEmployerHas(string freezeType)
    {
        var learningKey = GetShortCourseLearningKey();
        var approvalsApprenticeshipId = await GetApprovalsApprenticeshipId(learningKey);

        if (freezeType.Equals("Paused", StringComparison.OrdinalIgnoreCase))
        {
            await _testContext.TestFunction!.PublishEvent(new ApprenticeshipPausedEvent
            {
                ApprenticeshipId = approvalsApprenticeshipId,
                PausedOn = DateTime.UtcNow
            });
            return;
        }

        if (freezeType.Equals("Stopped", StringComparison.OrdinalIgnoreCase))
        {
            await _testContext.TestFunction!.PublishEvent(new ApprenticeshipStoppedEvent
            {
                ApprenticeshipId = approvalsApprenticeshipId,
                StopDate = DateTime.UtcNow.Date,
                AppliedOn = DateTime.UtcNow,
                ProviderId = await GetUkprn(learningKey)
            });
            return;
        }

        throw new ArgumentException($"Unknown freeze type '{freezeType}'. Expected Pause or Stop.");
    }

    [Then("the short course learning is marked as 'payments frozen'")]
    public async Task ThenTheShortCourseLearningIsMarkedAsPaymentsFrozen()
    {
        var learningKey = GetShortCourseLearningKey();

        await WaitHelper.WaitForIt(async () => await GetPaymentsFrozenFlag(learningKey),
            "Failed to mark short course episode as payments frozen");
    }

    [Then("a paymentStatus updated event is published with a frozen flag set to (.*)")]
    public async Task ThenAPaymentStatusUpdatedEventIsPublishedWithAFrozenFlagSetToTrue(bool paymentStatus)
    {
        var learningKey = GetShortCourseLearningKey();

        await WaitHelper.WaitForIt(() =>
                _testContext.MessageSession.ReceivedEvents<PaymentsStatusUpdatedForEpisode>()
                    .Any(e => IsExpectedPaymentsStatusUpdatedEvent(e, learningKey)),
            $"Failed to find published {nameof(PaymentsStatusUpdatedForEpisode)} event for learning key {learningKey}");

        var @event = _testContext.MessageSession.ReceivedEvents<PaymentsStatusUpdatedForEpisode>()
            .Last(e => IsExpectedPaymentsStatusUpdatedEvent(e, learningKey));

        @event.PaymentsFrozen.Should().Be(paymentStatus);
    }

    private Guid GetShortCourseLearningKey()
    {
        return new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearning].ToString()!);
    }

    private async Task<long> GetApprovalsApprenticeshipId(Guid learningKey)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        return await dbConnection.QuerySingleAsync<long>(
            @"SELECT TOP 1 ApprovalsApprenticeshipId
              FROM dbo.ShortCourseEpisode
              WHERE LearningKey = @learningKey",
            new { learningKey });
    }

    private async Task<bool> GetPaymentsFrozenFlag(Guid learningKey)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        return await dbConnection.QuerySingleAsync<bool>(
            @"SELECT TOP 1 PaymentsFrozen
              FROM dbo.ShortCourseEpisode
              WHERE LearningKey = @learningKey",
            new { learningKey });
    }

    private async Task<long> GetUkprn(Guid learningKey)
    {
            await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
            return await dbConnection.QuerySingleAsync<long>(
                    @"SELECT TOP 1 Ukprn
                        FROM dbo.ShortCourseEpisode
                        WHERE LearningKey = @learningKey",
                    new { learningKey });
    }

    private static bool IsExpectedPaymentsStatusUpdatedEvent(PaymentsStatusUpdatedForEpisode @event, Guid learningKey)
    {
        return @event.PaymentsFrozen && @event.LearningKey == learningKey;
    }

}