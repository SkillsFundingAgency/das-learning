using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class ProgressionInSubsequentAYStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private const long ProviderUkprn = 10005001;
    private const long LearnerUln = 123213;

    public ProgressionInSubsequentAYStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"a short course (SC-\S+) was (completed|withdrawn) by the learner in a prior academic year")]
    public async Task GivenAShortCourseWasEndedByTheLearnerInAPriorAcademicYear(string courseCode, string endType)
    {
        var onProgramme = endType == "completed"
            ? BuildOnProgramme(courseCode, completionDate: new DateTime(2025, 6, 1), milestones: [Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete])
            : BuildOnProgramme(courseCode, withdrawalDate: new DateTime(2025, 6, 1));

        var request = new CreateDraftShortCourseRequest
        {
            Ukprn = ProviderUkprn,
            AcademicYear = 2425,
            OnProgramme = [onProgramme],
            LearnerUpdateDetails = BuildLearnerDetails(),
            LearningSupport = []
        };

        (var responseBody, _) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", request);
        var result = responseBody?.Results.SingleOrDefault();

        _scenarioContext[ShortCourseTestKeys.ShortCourseLearner] = result?.LearnerKey ?? Guid.Empty;
    }

    [When(@"SLD POSTs a new course (SC-\S+) in the subsequent academic year")]
    public async Task WhenSLDPOSTsANewCourseInTheSubsequentAcademicYear(string courseCode)
    {
        var subsequentAYOnProgramme = new OnProgramme
        {
            Ukprn = ProviderUkprn,
            CourseCode = courseCode,
            StartDate = new DateTime(2025, 9, 1),
            ExpectedEndDate = new DateTime(2026, 6, 30),
            EmployerId = 99999999,
            Price = 1000,
            Milestones = []
        };

        var request = new CreateDraftShortCourseRequest
        {
            Ukprn = ProviderUkprn,
            AcademicYear = 2526,
            OnProgramme = [subsequentAYOnProgramme],
            LearnerUpdateDetails = BuildLearnerDetails(),
            LearningSupport = []
        };

        await _testContext.TestInnerApi.Post<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", request);
    }

    private static OnProgramme BuildOnProgramme(
        string courseCode,
        DateTime? completionDate = null,
        DateTime? withdrawalDate = null,
        List<Milestone>? milestones = null) => new()
    {
        Ukprn = ProviderUkprn,
        CourseCode = courseCode,
        StartDate = new DateTime(2024, 9, 1),
        ExpectedEndDate = new DateTime(2025, 6, 30),
        CompletionDate = completionDate,
        WithdrawalDate = withdrawalDate,
        EmployerId = 99999999,
        Price = 1000,
        Milestones = milestones ?? []
    };

    private static ShortCourseLearnerUpdateDetails BuildLearnerDetails() => new()
    {
        FirstName = "Frank",
        LastName = "Frankinson",
        DateOfBirth = new DateTime(2000, 1, 1),
        Uln = LearnerUln,
        LearnerRef = "LR-123213"
    };
}
