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
        var learning = await GetCurrentApprenticeshipLearning();
        var ukprn = learning.Episodes.First().Ukprn;
        var learningKey = learning.Key;
        await _testContext.TestInnerApi.Delete($"/{ukprn}/{learningKey}");

    }

    [Then(@"“last day of learning” for the Learning is set to its “Learning Start Date”")]
    public async Task ThenLastDayOfLearningForTheLearningIsSetToItsLearningStartDate()
    {
        var learning = await GetCurrentApprenticeshipLearning();
        learning.Episodes.First().WithdrawalDate = learning.Episodes.Select(e=>e.Prices.Select(p=>p.StartDate)).Min()?.First();
    }

    [Then(@"an LearningRemovedEvent is sent")]
    public async Task ThenALearningRemovedEventIsSent()
    {
        var learningKey = await GetLearningKey();
        await WaitHelper.WaitForIt(() =>
            _testContext.MessageSession.ReceivedEvents<LearningRemovedEvent>().Any(
                x => x.LearningKey == learningKey
            ), $"Failed to find published {nameof(LearningRemovedEvent)} event");
    }

    [Then(@"an LearningRemovedEvent is not sent")]
    public async Task ThenALearningRemovedEventIsNotSent()
    {
        var learningKey = await GetLearningKey();
        await WaitHelper.WaitForUnexpected(() =>
            _testContext.MessageSession.ReceivedEvents<LearningRemovedEvent>().Any(
                x => x.LearningKey == learningKey
            ), $"{nameof(LearningRemovedEvent)} event should not be published");
    }

    private async Task<Guid> GetLearningKey()
    {
        if (_scenarioContext.TryGetValue("ShortCourseLearningKey", out var shortCourseKey))
            return new Guid(shortCourseKey.ToString()!);

        var learning = await GetCurrentApprenticeshipLearning();
        return learning.Key;
    }

    private async Task<DataAccess.Entities.Learning.ApprenticeshipLearning> GetCurrentApprenticeshipLearning()
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        return dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
    }
}
