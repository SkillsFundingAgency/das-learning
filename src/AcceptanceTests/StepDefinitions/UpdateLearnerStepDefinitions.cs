using AutoFixture;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class UpdateLearnerStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;

    public UpdateLearnerStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
    }

    [Given(@"an update request has the following data")]
    public void ConfigureUpdateRequest(Table table)
    {
        var updateRequest = _scenarioContext.GetUpdateLearnerRequest();

        foreach (var row in table.Rows)
        {
            var propertyName = row["Property"];
            var valueString = row["Value"];

            switch(propertyName)
            {
                case "CompletionDate":
                    updateRequest.Learner.CompletionDate = TokenisableDateTime.FromString(valueString).Value;
                    break;
                case "WithdrawalDate":
                    updateRequest.Delivery.WithdrawalDate = TokenisableDateTime.FromString(valueString).Value;
                    break;


                default:
                    throw new ArgumentException($"Property '{propertyName}' is not recognized.");
            }
        }
    }

    [When(@"the update request is sent")]
    public async Task WhenTheUpdateRequestIsSent()
    {
        var updateRequest = _scenarioContext.GetUpdateLearnerRequest();
        var learningKey = _scenarioContext.GetLearningKey();
        var updateResponse =  await _testContext.TestInnerApi.Put<UpdateLearnerRequest, UpdateLearnerResult>($"/{learningKey}", updateRequest);
        _scenarioContext.SetUpdateLearnerResult(updateResponse);
    }

    [When(@"an update request is sent again with the same data")]
    public async Task WhenAnUpdateRequestIsSentWithTheSameData()
    {
        await Task.Delay(1000); // Ensure any previous events have been processed
        _testContext.MessageSession.ClearEventsOfType<LearningWithdrawnEvent>();

        await WhenTheUpdateRequestIsSent();
    }

    [Then(@"the “last day of learning” for the Learning is set to (.*)")]
    public async Task ThenTheLastDayOfLearningForTheLearningIsSetToTheProvidedWithdrawalDate(TokenisableDateTime withdrawDate)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        learning.Episodes.First().LastDayOfLearning = withdrawDate.Value;
    }

    [Then(@"the Earnings for the Learning are recalculated")]
    public void ThenTheEarningsForTheLearningAreRecalculated()
    {
        _scenarioContext.Pending();
    }

    [Then(@"the following changes are returned")]
    public void ValidateChangesReturned(Table table)
    {
        var changes = _scenarioContext.GetUpdateLearnerResult().Changes;

        var expectedChanges = table.Rows
            .Select(row => Enum.Parse<LearningUpdateChanges>(row["Change"]))
            .ToList();

        changes.Should().BeEquivalentTo(expectedChanges,
            "the returned changes should exactly match the expected changes");
    }

}
