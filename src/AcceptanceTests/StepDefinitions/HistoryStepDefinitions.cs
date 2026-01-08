using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.Learning.AcceptanceTests.Helpers;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class HistoryStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;

    public HistoryStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then("the learning history is maintained")]
    public async Task AssertHistoryUpdated()
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        var histories = dbConnection.GetHistories(learning.Key);

        if (histories.Count == 0)
        {
            Assert.Fail("No learning history created");
        }
    }
}