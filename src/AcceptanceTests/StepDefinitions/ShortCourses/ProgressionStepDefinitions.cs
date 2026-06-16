using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class ProgressionStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private const long ProviderUkprn = 10005001;
    private const string EndedOnProgrammeKey = "EndedOnProgramme";

    public ProgressionStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"an approved short course exists for course (.*)")]
    public async Task GivenAnApprovedShortCourseExistsForCourse(string courseCode)
    {
        var request = BuildRequest(courseCode);
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResult?>("/shortCourses", request);

        var learningKey = responseBody?.LearningKey ?? Guid.Empty;
        var learnerKey = responseBody?.LearnerKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseLearning] = learningKey;
        _scenarioContext[ShortCourseTestKeys.ShortCourseLearner] = learnerKey;
        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        dbConnection.SetAllEpisodesForShortCourseToApproved(learningKey);
    }

    [Given(@"the learner has (completed|withdrawn) (.*)")]
    public async Task GivenTheLearnerHasEndedCourse(string endType, string courseCode)
    {
        var onProgramme = endType == "completed"
            ? BuildOnProgramme(courseCode, completionDate: new DateTime(2025, 6, 1))
            : BuildOnProgramme(courseCode, withdrawalDate: new DateTime(2025, 6, 1));

        _scenarioContext[EndedOnProgrammeKey] = onProgramme;

        var updateRequest = new UpdateShortCourseRequest
        {
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [onProgramme]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [When(@"SLD submits a progression PUT for new course (.*)")]
    public async Task WhenSLDSubmitsAProgressionPUTForNewCourse(string newCourseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[EndedOnProgrammeKey];
        var updateRequest = new UpdateShortCourseRequest
        {
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [endedOnProgramme, BuildOnProgramme(newCourseCode)]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [When(@"SLD sends a progression PUT with (.*) completed and new course (.*)")]
    public async Task WhenSLDSendsAProgressionPUTWithCourseCompletedAndNewCourse(string completedCourseCode, string newCourseCode)
    {
        var learnerKey = GetLearnerKey();
        var updateRequest = new UpdateShortCourseRequest
        {
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme =
            [
                BuildOnProgramme(completedCourseCode, completionDate: new DateTime(2025, 6, 1)),
                BuildOnProgramme(newCourseCode)
            ]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{learnerKey}", updateRequest);
    }

    [Then(@"(\d+) short course learnings exist for the learner")]
    public async Task ThenNShortCourseLearningsExistForTheLearner(int expectedCount)
    {
        var learnerKey = GetLearnerKey();
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(learnerKey);
        learnings.Should().HaveCount(expectedCount);
    }

    [Then(@"a learning exists for course (.*)")]
    public async Task ThenALearningExistsForCourse(string courseCode)
    {
        var learnerKey = GetLearnerKey();
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(learnerKey);
        var found = learnings.Any(l =>
        {
            var learning = dbConnection.GetShortCourseLearning(l.Key);
            return learning.Episodes.Any(e => e.TrainingCode == courseCode);
        });
        found.Should().BeTrue($"expected a learning with course code {courseCode} to exist");
    }

    private Guid GetLearnerKey() => new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearner].ToString()!);

    private static OnProgramme BuildOnProgramme(string courseCode, DateTime? completionDate = null, DateTime? withdrawalDate = null) => new()
    {
        Ukprn = ProviderUkprn,
        CourseCode = courseCode,
        StartDate = new DateTime(2025, 1, 1),
        ExpectedEndDate = new DateTime(2025, 6, 30),
        CompletionDate = completionDate,
        WithdrawalDate = withdrawalDate,
        EmployerId = 99999999,
        Price = 1000,
        Milestones = new List<Milestone>()
    };

    private static ShortCourseLearnerUpdateDetails BuildLearnerDetails() => new()
    {
        FirstName = "Frank",
        LastName = "Frankinson",
        DateOfBirth = new DateTime(2000, 1, 1),
        Uln = 123213,
        LearnerRef = "LR-123213"
    };

    private static CreateDraftShortCourseRequest BuildRequest(string courseCode) => new()
    {
        OnProgramme = BuildOnProgramme(courseCode),
        LearnerUpdateDetails = BuildLearnerDetails(),
        LearningSupport = new List<LearningSupportDetails>()
    };
}
