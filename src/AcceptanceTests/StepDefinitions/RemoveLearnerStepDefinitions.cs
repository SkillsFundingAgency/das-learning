using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class RemoveLearnerStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private ApprenticeshipCreatedEvent ApprovalCreatedEvent => (ApprenticeshipCreatedEvent)_scenarioContext["ApprovalCreatedEvent"];

    public RemoveLearnerStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When(@"SLD inform us that a learner is to be removed")]
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
    public async Task ThenTheEarningsForTheLearningAreRecalculated()
    {
        var learning = await GetCurrentLearning();
        await WaitHelper.WaitForIt(() => 
            _testContext.MessageSession.ReceivedEvents<LearningWithdrawnEvent>().Any(
                x => x.LearningKey == learning.Key
            ), $"Failed to find published {nameof(LearningWithdrawnEvent)} event");

    }

    private async Task<DataAccess.Entities.Learning.Learning> GetCurrentLearning() 
    {
        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
        var learning = dbConnection.GetAll<DataAccess.Entities.Learning.Learning>().Single(x => x.Uln == ApprovalCreatedEvent.Uln);
        learning.Episodes = dbConnection.GetAll<DataAccess.Entities.Learning.Episode>().Where(x => x.LearningKey == learning.Key).ToList();

        foreach (var episode in learning.Episodes)
        {
            episode.Prices = dbConnection.GetAll<DataAccess.Entities.Learning.EpisodePrice>().Where(x => x.EpisodeKey == episode.Key).ToList();
            episode.LearningSupport = dbConnection.GetAll<DataAccess.Entities.Learning.LearningSupport>().Where(x => x.EpisodeKey == episode.Key).ToList();
        }

        return learning;
    }
}
