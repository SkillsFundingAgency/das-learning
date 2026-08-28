using AutoFixture;
using FluentAssertions;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using System.Net;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class CheckApprovedApprenticeshipExistsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;
    private readonly LearningDataSeeder _learningDataSeeder;
    private const string StatusCodeKey = "CheckApprovedApprenticeshipExistsStatusCode";

    public CheckApprovedApprenticeshipExistsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
        _learningDataSeeder = new LearningDataSeeder(_scenarioContext, _testContext, _fixture);
    }

    [Given(@"a provider has an approved apprenticeship")]
    public async Task GivenAProviderHasAnApprovedApprenticeship()
    {
        var createdEvent = await _learningDataSeeder.CreateLearner(
            actualStartDate: DateTime.Today.AddMonths(-1),
            endDate: DateTime.Today.AddYears(1),
            trainingPrice: 6000,
            epaPrice: 500);

        _scenarioContext.Set(createdEvent);
    }

    [When(@"the CheckApprovedApprenticeshipExists endpoint is called for that apprenticeship")]
    public async Task WhenTheCheckApprovedApprenticeshipExistsEndpointIsCalledForThatApprenticeship()
    {
        var createdEvent = _scenarioContext.Get<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>();

        var statusCode = await CallCheckApprovedApprenticeshipExists(
            createdEvent.Uln,
            createdEvent.TrainingCode,
            createdEvent.ActualStartDate!.Value);

        _scenarioContext.Set(statusCode, StatusCodeKey);
    }

    [When(@"the CheckApprovedApprenticeshipExists endpoint is called for an apprenticeship that was never created")]
    public async Task WhenTheCheckApprovedApprenticeshipExistsEndpointIsCalledForAnApprenticeshipThatWasNeverCreated()
    {
        var statusCode = await CallCheckApprovedApprenticeshipExists(
            _scenarioContext.GetNextUln().ToString(),
            _fixture.Create<int>().ToString(),
            DateTime.Today);

        _scenarioContext.Set(statusCode, StatusCodeKey);
    }

    [Then(@"the response status code is (\d+)")]
    public void ThenTheResponseStatusCodeIs(int expectedStatusCode)
    {
        var statusCode = _scenarioContext.Get<HttpStatusCode>(StatusCodeKey);
        ((int)statusCode).Should().Be(expectedStatusCode);
    }

    private async Task<HttpStatusCode> CallCheckApprovedApprenticeshipExists(string uln, string trainingCode, DateTime startDate)
    {
        var route = $"{Constants.UkPrn}/apprenticeships?uln={uln}&trainingCode={trainingCode}&startDate={startDate:yyyy-MM-dd}&isApproved=true";
        return await _testContext.TestInnerApi.Head(route);
    }
}
