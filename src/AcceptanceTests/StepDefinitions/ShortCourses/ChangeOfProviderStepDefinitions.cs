using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class ChangeOfProviderStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private const string ProviderAUkprnKey = "ProviderAUkprn";
    private const string ProviderBUkprnKey = "ProviderBUkprn";

    private const long ProviderAUkprn = 10005001;
    private const long ProviderBUkprn = 10005002;

    public ChangeOfProviderStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"a short course exists with Provider A")]
    public async Task GivenAShortCourseExistsWithProviderA()
    {
        var request = BuildRequest(ProviderAUkprn);
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResult?>("/shortCourses", request);

        var learningKey = responseBody?.LearningKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseLearning] = learningKey;
        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;
        _scenarioContext.Set(responseBody);
        _scenarioContext[ProviderAUkprnKey] = ProviderAUkprn;

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        dbConnection.SetAllEpisodesForShortCourseToApproved(learningKey);
    }

    [Given(@"a Change of Provider has occurred, creating an episode with Provider B")]
    [When(@"a Change of Provider has occurred, creating an episode with Provider B")]
    public async Task AChangeOfProviderHasOccurredToProviderB()
    {
        var request = BuildRequest(ProviderBUkprn);
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResult?>("/shortCourses", request);

        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;
        _scenarioContext.Set(responseBody);
        _scenarioContext[ProviderBUkprnKey] = ProviderBUkprn;

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        dbConnection.SetAllEpisodesForShortCourseToApproved(GetLearningKey());
    }

    [Then(@"the short course has (\d+) episodes?")]
    public async Task ThenTheShortCourseHasEpisodes(int expectedCount)
    {
        var learningKey = GetLearningKey();
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var shortCourse = dbConnection.GetShortCourseLearning(learningKey);
        shortCourse.Episodes.Should().HaveCount(expectedCount);
    }

    [When(@"SLD withdraws (Provider A|Provider B) from the short course")]
    public async Task WhenSLDWithdrawsProviderFromTheShortCourse(string providerName)
    {
        var request = BuildRequest(ResolveUkprn(providerName));
        request.OnProgramme.WithdrawalDate = new DateTime(2025, 1, 1);
        await CallUpdateShortCourseEndpoint(request);
    }

    [When(@"SLD calls the update short course endpoint for (Provider A|Provider B) with the 30% milestone")]
    public async Task WhenSLDCallsUpdateForProviderWithThirtyPercentMilestone(string providerName)
    {
        var request = BuildRequest(ResolveUkprn(providerName));
        request.OnProgramme.Milestones = new List<Milestone> { Milestone.ThirtyPercentLearningComplete };
        await CallUpdateShortCourseEndpoint(request);
    }

    [When(@"SLD removes the short course for (Provider A|Provider B)")]
    [Given(@"SLD has removed the short course for (Provider A|Provider B)")]
    public async Task SLDRemovesTheShortCourseForProvider(string providerName)
    {
        var ukprn = ResolveUkprn(providerName);
        await _testContext.TestInnerApi.Delete($"/{ukprn}/shortCourses/{GetLearningKey()}");
    }

    [When(@"SLD reinstates the short course for (Provider A|Provider B)")]
    public async Task WhenSLDReinstatesTheShortCourseForProvider(string providerName)
    {
        var request = BuildRequest(ResolveUkprn(providerName));
        await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResult?>("/shortCourses", request);
    }

    [Then(@"(Provider A|Provider B)'s episode is withdrawn")]
    public async Task ThenProviderEpisodeIsWithdrawn(string providerName)
    {
        var episode = await GetEpisodeForProvider(providerName);
        episode.WithdrawalDate.Should().NotBeNull();
    }

    [Then(@"(Provider A|Provider B)'s episode is not withdrawn")]
    public async Task ThenProviderEpisodeIsNotWithdrawn(string providerName)
    {
        var episode = await GetEpisodeForProvider(providerName);
        episode.WithdrawalDate.Should().BeNull();
    }

    [Then(@"(Provider A|Provider B)'s episode has the milestone recorded")]
    public async Task ThenProviderEpisodeHasMilestoneRecorded(string providerName)
    {
        var episode = await GetEpisodeForProvider(providerName);
        episode.Milestones.Should().NotBeEmpty();
    }

    [Then(@"(Provider A|Provider B)'s episode has no milestone")]
    public async Task ThenProviderEpisodeHasNoMilestone(string providerName)
    {
        var episode = await GetEpisodeForProvider(providerName);
        episode.Milestones.Should().BeEmpty();
    }

    [Then(@"(Provider A|Provider B)'s episode IsRemoved is (True|False)")]
    public async Task ThenProviderEpisodeIsRemovedIs(string providerName, bool isRemoved)
    {
        var episode = await GetEpisodeForProvider(providerName);
        episode.IsRemoved.Should().Be(isRemoved);
    }

    [Given(@"Provider B has completed the short course")]
    public async Task GivenProviderBHasCompletedTheShortCourse()
    {
        var request = BuildRequest(ProviderBUkprn);
        request.OnProgramme.CompletionDate = new DateTime(2024, 12, 1);
        await CallUpdateShortCourseEndpoint(request);
    }

    [When(@"SLD calls the update short course endpoint for (Provider A|Provider B)")]
    public async Task WhenSLDCallsUpdateForProvider(string providerName)
    {
        var request = BuildRequest(ResolveUkprn(providerName));
        request.OnProgramme.StartDate = new DateTime(2024, 1, 2);
        await CallUpdateShortCourseEndpoint(request);
    }

    [Then(@"the short course status is Complete")]
    public async Task ThenTheShortCourseStatusIsComplete()
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var shortCourse = dbConnection.GetShortCourseLearning(GetLearningKey());
        shortCourse.CompletionDate.Should().NotBeNull();
    }

    private long ResolveUkprn(string providerName) =>
        providerName == "Provider A"
            ? (long)_scenarioContext[ProviderAUkprnKey]
            : (long)_scenarioContext[ProviderBUkprnKey];

    private Guid GetLearningKey() => new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearning].ToString()!);

    private async Task<DataAccess.Entities.Learning.ShortCourseEpisode> GetEpisodeForProvider(string providerName)
    {
        var ukprn = ResolveUkprn(providerName);
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var shortCourse = dbConnection.GetShortCourseLearning(GetLearningKey());
        return shortCourse.Episodes.Single(e => e.Ukprn == ukprn);
    }

    private async Task CallUpdateShortCourseEndpoint(CreateDraftShortCourseRequest request)
    {
        await _testContext.TestInnerApi.Put<CreateDraftShortCourseRequest, object>($"/shortCourses/{GetLearningKey()}", request);
    }

    private static CreateDraftShortCourseRequest BuildRequest(long ukprn) => new()
    {
        OnProgramme = new OnProgramme
        {
            Ukprn = ukprn,
            StartDate = new DateTime(2024, 1, 1),
            ExpectedEndDate = new DateTime(2024, 12, 1),
            WithdrawalDate = null,
            CompletionDate = null,
            CourseCode = "SC-ART1",
            EmployerId = 99999999,
            Price = 1000,
            Milestones = new List<Milestone>()
        },
        LearnerUpdateDetails = new ShortCourseLearnerUpdateDetails
        {
            FirstName = "Frank",
            LastName = "Frankinson",
            DateOfBirth = new DateTime(2000, 1, 1),
            Uln = 123213,
            LearnerRef = "LR-123213"
        },
        LearningSupport = new List<LearningSupportDetails>()
    };
}
