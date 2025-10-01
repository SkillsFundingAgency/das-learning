using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class RemoveLearnerStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public RemoveLearnerStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When(@"SLD inform us that a learner is to be removed")]
    [Given(@"SLD have previously informed us that the learner is to be removed")]
    public async Task WhenSLDInformUsThatALearnerIsToBeRemoved()
    {
        var learning = await GetCurrentLearning();
        var ukprn = learning.Episodes.First().Ukprn;
        var learningKey = learning.Key;
        await _testContext.TestInnerApi.Delete($"/{ukprn}/{learningKey}");

    }

    [Then(@"the Completion Status for the Learning is set to “Withdrawn”")]
    public async Task ThenTheCompletionStatusForTheLearningIsSetToWithdrawn()
    {
        var learning = await GetCurrentLearning();
        learning.Episodes.First().LearningStatus.Should().Be("Withdrawn");
    }

    [Then(@"“last day of learning” for the Learning is set to its “Learning Start Date”")]
    public async Task ThenLastDayOfLearningForTheLearningIsSetToItsLearningStartDate()
    {
        var learning = await GetCurrentLearning();
        learning.Episodes.First().LastDayOfLearning = learning.Episodes.Select(e=>e.Prices.Select(p=>p.StartDate)).Min()?.First();
    }

    [Then(@"a LearningWithdrawnEvent is sent")]
    public async Task ThenALearningWithdrawnEventIsSent()
    {
        var learning = await GetCurrentLearning();
        await WaitHelper.WaitForIt(() => 
            _testContext.MessageSession.ReceivedEvents<LearningWithdrawnEvent>().Any(
                x => x.LearningKey == learning.Key
            ), $"Failed to find published {nameof(LearningWithdrawnEvent)} event");

    }

    [Then(@"a LearningWithdrawnEvent is not sent")]
    public async Task ThenALearningWithdrawnEventIsNotSent()
    {
        var learning = await GetCurrentLearning();
        await WaitHelper.WaitForUnexpected(() =>
            _testContext.MessageSession.ReceivedEvents<LearningWithdrawnEvent>().Any(
                x => x.LearningKey == learning.Key
            ), $"{nameof(LearningWithdrawnEvent)} event should not be published");

    }

    private async Task<DataAccess.Entities.Learning.Learning> GetCurrentLearning() 
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        return dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
    }
}
