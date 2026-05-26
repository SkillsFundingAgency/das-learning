using AutoFixture;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Models.Dtos;
using System.Text.Json;

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

        // We create a learner that will definitely be included in the response. This is needed because if no learners are returned we get a 404
        await _learningDataSeeder.CreateLearner(startDate.DateTime!.Value.AddYears(-5), plannedEndDate.DateTime!.Value.AddYears(5), trainingPrice, epaPrice);

        var approvalCreatedEvent = await _learningDataSeeder.CreateLearner(startDate.DateTime!.Value, plannedEndDate.DateTime!.Value, trainingPrice, epaPrice);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        Console.WriteLine(JsonSerializer.Serialize(approvalCreatedEvent, options));

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

    [Then(@"the fm36 learner should be (.*)")]
    public void VerifyResponse(string expectedResult)
    {
        var response = _scenarioContext.Get<List<LearningWithEpisodes>>(GetFm36LearnersResponse);

        // We assert on 2 records as we have added a control record to ensure a 404 is not returned from the endpoint
        if (expectedResult == "Included")
        {
            Assert.IsTrue(response.Count() == 2, "Expected learner was not included in response");
        }
        else
        {
            Assert.IsFalse(response.Count() == 2, "Learner was included in response but should have been excluded");
        }
    }
    
}