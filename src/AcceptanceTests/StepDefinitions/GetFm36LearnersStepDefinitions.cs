using AutoFixture;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Models.Dtos;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class GetFm36LearnersStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;
    private readonly LearningDataSeeder _learningDataSeeder;
    private const string GetFm36LearnersResponse = "GetFm36LearnersResponse";

    public GetFm36LearnersStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
        _learningDataSeeder = new LearningDataSeeder(_scenarioContext, _testContext, _fixture);
    }

    [Given(@"The learner starts on (.*) and has a plannedEndDate of (.*)")]
    public async Task CreateLearner(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate)
    {
        var trainingPrice = 6000m;
        var epaPrice = 500m;

        var approvalCreatedEvent = await _learningDataSeeder.CreateLearner(startDate.DateTime!.Value, plannedEndDate.DateTime!.Value, trainingPrice, epaPrice);

        _scenarioContext.SetApprenticeshipCreatedEvent(approvalCreatedEvent);

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnerKey = dbConnection.GetLearningKey(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        _scenarioContext.SetLearningKey(learnerKey);
    }

    [Given(@"a withdrawn date of (.*) is set")]
    public void SetWithdrawalDate(TokenisableDateTime withdrawalDate)
    {
        if (withdrawalDate.DateTime.HasValue)
        {
            var updateRequest = _scenarioContext.GetUpdateLearnerRequest();
            updateRequest.Delivery.WithdrawalDate = withdrawalDate.DateTime;
        }
    }

    [Given(@"a completion date of (.*) is set")]
    public void GivenACompletionDateOfNullIsSet(TokenisableDateTime completionDate)
    {
        if (completionDate.DateTime.HasValue)
        {
            var updateRequest = _scenarioContext.GetUpdateLearnerRequest();
            updateRequest.Learner.CompletionDate = completionDate.DateTime;
        }
    }

    [When(@"the GetLearningsForFm36 endpoint is called for (.*)")]
    public async Task RetrieveLearnersForCurrentDate(TokenisableDateTime requestedDate)
    {
        var collectionYear = requestedDate.AcademicYear();
        var collectionPeriod = requestedDate.CollectionPeriod();

        var response = await _testContext.TestInnerApi.Get<List<LearningWithEpisodes>>($"{Constants.UkPrn}/{collectionYear}/{collectionPeriod}");
        _scenarioContext.Set(response, GetFm36LearnersResponse);
    }

    [Then(@"the learner should be (.*)")]
    public void VerifyResponse(string expectedResult)
    {
        var response = _scenarioContext.Get<List<LearningWithEpisodes>>(GetFm36LearnersResponse);

        if (expectedResult == "Included")
        {
            Assert.IsTrue(response.Count() == 1, "Expected a learner to be included in the response.");
        }
        else
        {
            Assert.IsFalse(response.Count() == 1, "Expected no learners to be included in the response.");
        }
    }
    
}