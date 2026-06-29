using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class MultiFeatureStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private const long ProviderAUkprn = 10005001;
    private const long ProviderBUkprn = 10005002;

    public MultiFeatureStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"Provider B has POSTed new course (.*)")]
    public async Task GivenProviderBHasPOSTedNewCourse(string courseCode)
    {
        var request = BuildCreateRequest(courseCode, ProviderBUkprn);
        await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", request);
    }

    [Given(@"Provider B has withdrawn (SC-\d+)")]
    public async Task GivenProviderBHasWithdrawnCourse(string courseCode)
    {
        var updateRequest = new UpdateShortCourseRequest
        {
            Ukprn = ProviderBUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [BuildOnProgramme(courseCode, ProviderBUkprn, withdrawalDate: new DateTime(2025, 6, 1))]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [When(@"Provider A PUTs course (.*)")]
    public async Task WhenProviderAPUTsCourse(string courseCode)
    {
        var updateRequest = new UpdateShortCourseRequest
        {
            Ukprn = ProviderAUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [BuildOnProgramme(courseCode, ProviderAUkprn)]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [Then(@"(SC-\d+) has (\d+) episodes?")]
    public async Task ThenCourseHasEpisodes(string courseCode, int expectedCount)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        var learning = learnings.Single(l => l.TrainingCode == courseCode);
        var episodes = dbConnection.GetShortCourseLearning(learning.Key).Episodes;

        episodes.Should().HaveCount(expectedCount);
    }

    [Then(@"(SC-\d+)'s episodes belong to a single Learning")]
    public async Task ThenCourseEpisodesBelongToASingleLearning(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());

        learnings.Where(l => l.TrainingCode == courseCode).Should().HaveCount(1);
    }

    [Then(@"(SC-\d+)'s only episode belongs to (Provider A|Provider B)")]
    public async Task ThenCoursesOnlyEpisodeBelongsToProvider(string courseCode, string providerName)
    {
        var expectedUkprn = providerName == "Provider A" ? ProviderAUkprn : ProviderBUkprn;

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        var learning = learnings.Single(l => l.TrainingCode == courseCode);
        var episode = dbConnection.GetShortCourseLearning(learning.Key).Episodes.Single();

        episode.Ukprn.Should().Be(expectedUkprn);
    }

    private Guid GetLearnerKey() => new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearner].ToString()!);

    private static OnProgramme BuildOnProgramme(string courseCode, long ukprn, DateTime? withdrawalDate = null) => new()
    {
        Ukprn = ukprn,
        CourseCode = courseCode,
        StartDate = new DateTime(2025, 1, 1),
        ExpectedEndDate = new DateTime(2025, 6, 30),
        WithdrawalDate = withdrawalDate,
        EmployerId = 99999999,
        Price = 1000,
        Milestones = []
    };

    private static ShortCourseLearnerUpdateDetails BuildLearnerDetails() => new()
    {
        FirstName = "Frank",
        LastName = "Frankinson",
        DateOfBirth = new DateTime(2000, 1, 1),
        Uln = 123213,
        LearnerRef = "LR-123213"
    };

    private static CreateDraftShortCourseRequest BuildCreateRequest(string courseCode, long ukprn) => new()
    {
        Ukprn = ukprn,
        OnProgramme = [BuildOnProgramme(courseCode, ukprn)],
        LearnerUpdateDetails = BuildLearnerDetails(),
        LearningSupport = []
    };
}
