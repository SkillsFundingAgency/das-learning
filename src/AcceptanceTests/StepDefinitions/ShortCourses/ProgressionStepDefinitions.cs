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

    public ProgressionStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    // --- Setup steps ---

    [Given(@"an approved short course exists for course (.*)")]
    public async Task GivenAnApprovedShortCourseExistsForCourse(string courseCode)
    {
        var request = BuildRequest(courseCode);
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", request);
        var result = responseBody?.Results.SingleOrDefault();

        var learningKey = result?.LearningKey ?? Guid.Empty;
        var learnerKey = result?.LearnerKey ?? Guid.Empty;
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
            ? BuildOnProgramme(courseCode, completionDate: new DateTime(2025, 6, 1), milestones: [Milestone.LearningComplete, Milestone.ThirtyPercentLearningComplete])
            : BuildOnProgramme(courseCode, withdrawalDate: new DateTime(2025, 6, 1));

        _scenarioContext[ShortCourseTestKeys.EndedOnProgramme] = onProgramme;

        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [onProgramme]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [Given(@"the progression course (.*) is approved")]
    public async Task GivenTheProgressionCourseIsApproved(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        var learning = learnings.Single(l => l.TrainingCode == courseCode);
        dbConnection.SetAllEpisodesForShortCourseToApproved(learning.Key);
    }

    [Given(@"a progression PUT has added new course (.*)")]
    [When(@"SLD submits a progression PUT for new course (.*)")]
    public async Task WhenSLDSubmitsAProgressionPUTForNewCourse(string newCourseCode)
    {
        _scenarioContext[ShortCourseTestKeys.ProgressionCourseCode] = newCourseCode;
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [endedOnProgramme, BuildOnProgramme(newCourseCode)]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [When(@"SLD sends a progression PUT with (.*) completed and new course (.*)")]
    public async Task WhenSLDSendsAProgressionPUTWithCourseCompletedAndNewCourse(string completedCourseCode, string newCourseCode)
    {
        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme =
            [
                BuildOnProgramme(completedCourseCode, completionDate: new DateTime(2025, 6, 1)),
                BuildOnProgramme(newCourseCode)
            ]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    // --- Lifecycle action steps ---

    [When(@"SLD claims the 30% milestone on (.*)")]
    public async Task WhenSLDClaimsThirtyPercentMilestoneOn(string courseCode)
    {
        await PutWithCourseModified(courseCode, BuildOnProgramme(courseCode, milestones: [Milestone.ThirtyPercentLearningComplete]));
    }

    [When(@"^SLD withdraws (\S+)$")]
    public async Task WhenSLDWithdraws(string courseCode)
    {
        await PutWithCourseModified(courseCode, BuildOnProgramme(courseCode, withdrawalDate: new DateTime(2025, 9, 1)));
    }

    [When(@"SLD updates the completion date of (.*)")]
    public async Task WhenSLDUpdatesCompletionDateOf(string courseCode)
    {
        await PutWithCourseModified(courseCode, BuildOnProgramme(courseCode, completionDate: new DateTime(2025, 6, 15)));
    }

    [When(@"SLD updates the start date of (.*)")]
    public async Task WhenSLDUpdatesStartDateOf(string courseCode)
    {
        await PutWithCourseModified(courseCode, BuildOnProgramme(courseCode, startDate: new DateTime(2025, 3, 1)));
    }

    [Given(@"SLD has omitted (.*) from the next PUT")]
    [When(@"SLD omits (.*) from the next PUT")]
    public async Task WhenSLDOmitsCourseFromNextPUT(string omittedCourseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var progressionCourseCode = (string)_scenarioContext[ShortCourseTestKeys.ProgressionCourseCode];

        var onProgrammeItems = new List<OnProgramme>();
        if (endedOnProgramme.CourseCode != omittedCourseCode) onProgrammeItems.Add(endedOnProgramme);
        if (progressionCourseCode != omittedCourseCode) onProgrammeItems.Add(BuildOnProgramme(progressionCourseCode));

        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = onProgrammeItems
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    [When(@"SLD removes all learning for the learner")]
    public async Task WhenSLDRemovesTheShortCourse()
    {
        await _testContext.TestInnerApi.Delete($"/{ProviderUkprn}/shortCourses/{GetLearnerKey()}?academicYear=2425");
    }

    [When(@"SLD includes (.*) in the next PUT")]
    public async Task WhenSLDIncludesCourseInNextPUT(string courseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme = [endedOnProgramme, BuildOnProgramme(courseCode)]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    // --- Progression creation assertions ---

    [Then(@"(\d+) short course learnings exist for the learner")]
    public async Task ThenNShortCourseLearningsExistForTheLearner(int expectedCount)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        learnings.Should().HaveCount(expectedCount);
    }

    [Then(@"^a learning exists for course (\S+)$")]
    public async Task ThenALearningExistsForCourse(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        learnings.Should().Contain(l => l.TrainingCode == courseCode, $"expected a learning with course code {courseCode} to exist");
    }

    [Then(@"a learning exists for course (.*) with Ukprn (\d+)")]
    public async Task ThenALearningExistsForCourseWithUkprn(string courseCode, long ukprn)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        var learning = learnings.SingleOrDefault(l => l.TrainingCode == courseCode);
        learning.Should().NotBeNull($"expected a learning with course code {courseCode} to exist");
        var fullLearning = dbConnection.GetShortCourseLearning(learning!.Key);
        fullLearning.Episodes.Should().Contain(e => e.Ukprn == ukprn, $"expected an episode with Ukprn {ukprn} for course {courseCode}");
    }

    // --- Lifecycle isolation assertions ---

    [Then(@"(.*) has the 30% milestone")]
    public async Task ThenCourseHasThirtyPercentMilestone(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).Milestones.Should().NotBeEmpty();
    }

    [Then(@"(.*) has no 30% milestone")]
    public async Task ThenCourseHasNoThirtyPercentMilestone(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).Milestones
            .Should().NotContain(m => m.Milestone == Milestone.ThirtyPercentLearningComplete);
    }

    [Then(@"^(\S+) is withdrawn$")]
    public async Task ThenCourseIsWithdrawn(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).WithdrawalDate.Should().NotBeNull();
    }

    [Then(@"^(\S+) is not withdrawn$")]
    public async Task ThenCourseIsNotWithdrawn(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).WithdrawalDate.Should().BeNull();
    }

    [Then(@"(.*) has a completion date")]
    public async Task ThenCourseHasACompletionDate(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).CompletionDate.Should().NotBeNull();
    }

    [Then(@"(.*) has no completion date")]
    public async Task ThenCourseHasNoCompletionDate(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).CompletionDate.Should().BeNull();
    }

    [Then(@"(.*) has the updated start date")]
    public async Task ThenCourseHasUpdatedStartDate(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).StartDate.Should().Be(new DateTime(2025, 3, 1));
    }

    [Then(@"(.*) is removed")]
    public async Task ThenCourseIsRemoved(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).IsRemoved.Should().BeTrue();
    }

    [Then(@"(.*) is not removed")]
    public async Task ThenCourseIsNotRemoved(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        GetEpisodeForCourse(dbConnection, courseCode).IsRemoved.Should().BeFalse();
    }

    // --- Helpers ---

    private async Task PutWithCourseModified(string modifiedCourseCode, OnProgramme modifiedOnProgramme)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var progressionCourseCode = (string)_scenarioContext[ShortCourseTestKeys.ProgressionCourseCode];

        var updateRequest = new UpdateShortCourseRequest
        {
            AcademicYear = 2425,
            Ukprn = ProviderUkprn,
            LearnerUpdateDetails = BuildLearnerDetails(),
            OnProgramme =
            [
                endedOnProgramme.CourseCode == modifiedCourseCode ? modifiedOnProgramme : endedOnProgramme,
                progressionCourseCode == modifiedCourseCode ? modifiedOnProgramme : BuildOnProgramme(progressionCourseCode)
            ]
        };

        await _testContext.TestInnerApi.Put<UpdateShortCourseRequest, object>($"/shortCourses/{GetLearnerKey()}", updateRequest);
    }

    private DataAccess.Entities.Learning.ShortCourseEpisode GetEpisodeForCourse(SqlConnection dbConnection, string courseCode)
    {
        var learnings = dbConnection.GetShortCourseLearningsForLearner(GetLearnerKey());
        var learning = learnings.Single(l => l.TrainingCode == courseCode);
        return dbConnection.GetShortCourseLearning(learning.Key).Episodes.Single();
    }

    private Guid GetLearnerKey() => new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearner].ToString()!);

    private static OnProgramme BuildOnProgramme(string courseCode, DateTime? completionDate = null, DateTime? withdrawalDate = null, List<Milestone>? milestones = null, DateTime? startDate = null) => new()
    {
        Ukprn = ProviderUkprn,
        CourseCode = courseCode,
        StartDate = startDate ?? new DateTime(2025, 1, 1),
        ExpectedEndDate = new DateTime(2025, 6, 30),
        CompletionDate = completionDate,
        WithdrawalDate = withdrawalDate,
        EmployerId = 99999999,
        Price = 1000,
        Milestones = milestones ?? new List<Milestone>()
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
        Ukprn = ProviderUkprn,
        AcademicYear = 2425,
        OnProgramme = [BuildOnProgramme(courseCode)],
        LearnerUpdateDetails = BuildLearnerDetails(),
        LearningSupport = new List<LearningSupportDetails>()
    };
}
