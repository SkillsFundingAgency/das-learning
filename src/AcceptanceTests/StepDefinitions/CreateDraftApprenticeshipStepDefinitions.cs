using AutoFixture;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class CreateDraftApprenticeshipStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;

    public CreateDraftApprenticeshipStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
    }

    [When(@"CreateDraftApprenticeship is called with apprenticeship details")]
    public async Task WhenCreateDraftApprenticeshipIsCalledWithApprenticeshipDetails()
    {
        var createdEvent = _scenarioContext.GetApprenticeshipCreatedEvent();// this may or may not have been sent
        var updateRequest = _scenarioContext.GetUpdateLearnerRequest();
        var ukprn = createdEvent.ProviderId;
        var response = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftApprenticeship, CreateDraftApprenticeshipLearningCommandResult>($"/{ukprn}/apprenticeships", updateRequest);
        _scenarioContext.SetCreateDraftApprenticeshipLearningResult(response);
    }

    [Then(@"the CreateDraftApprenticeship endpoint should return a (.*)")]
    public void ThenTheCreateDraftApprenticeshipEndpointShouldReturnA(int expectedStatusCode)
    {
        (var result, var statusCode) = _scenarioContext.GetCreateDraftApprenticeshipLearningResult();
        statusCode.Should().Be((HttpStatusCode)expectedStatusCode);
    }

}
