using AutoFixture;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;
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

        // We create a control learner that is not requested, to show that only the requested learner is returned
        await _learningDataSeeder.CreateLearner(startDate.DateTime!.Value.AddYears(-5), plannedEndDate.DateTime!.Value.AddYears(5), trainingPrice, epaPrice);

        var approvalCreatedEvent = await _learningDataSeeder.CreateLearner(startDate.DateTime!.Value, plannedEndDate.DateTime!.Value, trainingPrice, epaPrice);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        Console.WriteLine(JsonSerializer.Serialize(approvalCreatedEvent, options));

        _scenarioContext.SetApprenticeshipCreatedEvent(approvalCreatedEvent);

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var uln = _scenarioContext.GetApprenticeshipCreatedEvent().Uln;
        _scenarioContext.SetLearningKey(dbConnection.GetLearningKey(uln));
        _scenarioContext.SetLearnerKey(dbConnection.GetLearner(uln).Key);
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

    [When(@"the GetLearningsForFm36 endpoint is called for the learner")]
    public async Task RetrieveLearnerByKey()
    {
        var learningKey = _scenarioContext.GetLearningKey();

        // The control learner created in the Given step is deliberately not requested
        var (response, _) = await _testContext.TestInnerApi.PostWithResponseCode<List<Guid>, List<LearningWithEpisodes>>(
            $"{Constants.UkPrn}/learnings/by-keys", new List<Guid> { learningKey });

        _scenarioContext.Set(response ?? new List<LearningWithEpisodes>(), GetFm36LearnersResponse);
    }

    [Then(@"the fm36 learner should be (.*)")]
    public void VerifyResponse(string expectedResult)
    {
        var response = _scenarioContext.Get<List<LearningWithEpisodes>>(GetFm36LearnersResponse);
        var learningKey = _scenarioContext.GetLearningKey();

        if (expectedResult == "Included")
        {
            // Only the requested learner is returned, not the control learner
            Assert.That(response.Select(x => x.Key), Is.EqualTo(new[] { learningKey }), "Expected only the requested learner to be returned");
        }
        else
        {
            Assert.That(response, Is.Empty, "Learner was included in response but should have been excluded");
        }
    }
}
