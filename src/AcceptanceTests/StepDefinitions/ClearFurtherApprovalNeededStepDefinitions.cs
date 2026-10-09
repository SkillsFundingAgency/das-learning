using FluentAssertions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using System.Net;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class ClearFurtherApprovalNeededStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
{
    private const string StatusCodeKey = "ClearFurtherApprovalNeededStatusCode";

    [Then(@"the update result says further approval is needed")]
    public void ThenTheUpdateResultSaysFurtherApprovalIsNeeded()
    {
        scenarioContext.GetUpdateLearnerResult().NeedsFurtherApproval.Should().BeTrue();
    }

    [Then(@"further approval needed is set on the apprenticeship episode")]
    public async Task ThenFurtherApprovalNeededIsSet()
    {
        (await GetEpisode()).FurtherApprovalNeeded.Should().BeTrue();
    }

    [Then(@"further approval needed is not set on the apprenticeship episode")]
    public async Task ThenFurtherApprovalNeededIsNotSet()
    {
        (await GetEpisode()).FurtherApprovalNeeded.Should().BeFalse();
    }

    [When(@"further approval needed is cleared for the apprenticeship episode")]
    public async Task WhenFurtherApprovalNeededIsCleared()
    {
        var episode = await GetEpisode();
        await Clear(episode.LearningKey, episode.Key);
    }

    [When(@"further approval needed is cleared for a learning that does not exist")]
    public async Task WhenFurtherApprovalNeededIsClearedForAnUnknownLearning()
    {
        await Clear(Guid.NewGuid(), Guid.NewGuid());
    }

    [When(@"further approval needed is cleared for an episode that does not exist")]
    public async Task WhenFurtherApprovalNeededIsClearedForAnUnknownEpisode()
    {
        var episode = await GetEpisode();
        await Clear(episode.LearningKey, Guid.NewGuid());
    }

    [Then(@"the clear request returns (\d+)")]
    public void ThenTheClearRequestReturns(int expectedStatusCode)
    {
        ((int)scenarioContext.Get<HttpStatusCode>(StatusCodeKey)).Should().Be(expectedStatusCode);
    }

    private async Task Clear(Guid learningKey, Guid episodeKey)
    {
        var route = $"/learning/{learningKey}/episodes/{episodeKey}/clear-further-approval-needed?learningType=Apprenticeship";
        var (_, statusCode) = await testContext.TestInnerApi.PostWithResponseCode<object?, object>(route, null);
        scenarioContext.Set(statusCode, StatusCodeKey);
    }

    private Task<DataAccess.Entities.Learning.ApprenticeshipEpisode> GetEpisode()
    {
        using var dbConnection = new SqlConnection(scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        return Task.FromResult(learning.Episodes.Single());
    }
}
