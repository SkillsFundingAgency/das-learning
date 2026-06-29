using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class ProgressionPreApprovalStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private const long ProviderUkprn = 10005001;

    public ProgressionPreApprovalStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"an unapproved short course exists for course (.*)")]
    public async Task GivenAnUnapprovedShortCourseExistsForCourse(string courseCode)
    {
        var request = BuildRequest(courseCode);
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", request);
        var result = responseBody?.Results.SingleOrDefault();

        _scenarioContext[ShortCourseTestKeys.ShortCourseLearning] = result?.LearningKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseLearner] = result?.LearnerKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;
    }

    [When(@"SLD POSTs ended course (.*) and new course (.*)")]
    public async Task WhenSLDPOSTsEndedCourseAndNewCourse(string endedCourseCode, string newCourseCode)
    {
        _scenarioContext[ShortCourseTestKeys.ProgressionCourseCode] = newCourseCode;
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var createRequest = BuildRequest(newCourseCode);
        createRequest.OnProgramme = [endedOnProgramme, createRequest.OnProgramme.Single()];

        await _testContext.TestInnerApi.Post<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", createRequest);
    }

    [Given(@"SLD has POSTed new course (.*)")]
    public async Task GivenSLDHasPOSTedNewCourse(string newCourseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        await WhenSLDPOSTsEndedCourseAndNewCourse(endedOnProgramme.CourseCode, newCourseCode);
    }

    [Given(@"SLD has omitted (.*) from the next POST")]
    [When(@"SLD omits (.*) from the next POST")]
    public async Task WhenSLDOmitsCourseFromNextPOST(string omittedCourseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        var progressionCourseCode = (string)_scenarioContext[ShortCourseTestKeys.ProgressionCourseCode];

        var onProgrammeItems = new List<OnProgramme>();
        if (endedOnProgramme.CourseCode != omittedCourseCode) onProgrammeItems.Add(endedOnProgramme);
        if (progressionCourseCode != omittedCourseCode) onProgrammeItems.Add(BuildOnProgramme(progressionCourseCode));

        var createRequest = BuildRequest(onProgrammeItems.First().CourseCode);
        createRequest.OnProgramme = onProgrammeItems;

        await _testContext.TestInnerApi.Post<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", createRequest);
    }

    [When(@"SLD includes (.*) in the next POST")]
    public async Task WhenSLDIncludesCourseInNextPOST(string courseCode)
    {
        var endedOnProgramme = (OnProgramme)_scenarioContext[ShortCourseTestKeys.EndedOnProgramme];
        await WhenSLDPOSTsEndedCourseAndNewCourse(endedOnProgramme.CourseCode, courseCode);
    }

    private const long BrandNewLearnerUln = 999999;

    [When(@"SLD POSTs a brand new learner with courses (.*) and (.*)")]
    public async Task WhenSLDPOSTsABrandNewLearnerWithCourses(string firstCourseCode, string secondCourseCode)
    {
        var createRequest = BuildRequest(firstCourseCode, BrandNewLearnerUln);
        createRequest.OnProgramme =
        [
            createRequest.OnProgramme.Single(),
            BuildRequest(secondCourseCode, BrandNewLearnerUln).OnProgramme.Single()
        ];

        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", createRequest);
        var result = responseBody?.Results.FirstOrDefault();

        _scenarioContext[ShortCourseTestKeys.ShortCourseLearner] = result?.LearnerKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;
    }

    [When(@"SLD POSTs a brand new learner with completed course (.*) and new course (.*)")]
    public async Task WhenSLDPOSTsABrandNewLearnerWithCompletedCourseAndNewCourse(string completedCourseCode, string newCourseCode)
    {
        var createRequest = BuildRequest(completedCourseCode, BrandNewLearnerUln);
        createRequest.OnProgramme =
        [
            BuildOnProgramme(completedCourseCode, completionDate: new DateTime(2026, 1, 15), milestones: [Milestone.ThirtyPercentLearningComplete]),
            BuildOnProgramme(newCourseCode)
        ];

        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateDraftShortCourseCommandResponse>("/shortCourses", createRequest);
        var result = responseBody?.Results.FirstOrDefault();

        _scenarioContext[ShortCourseTestKeys.ShortCourseLearner] = result?.LearnerKey ?? Guid.Empty;
        _scenarioContext[ShortCourseTestKeys.ShortCourseEndpointResponseCode] = (int)statusCode;
    }

    [Then(@"(.*) is unapproved")]
    public async Task ThenCourseIsUnapproved(string courseCode)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnerKey = new Guid(_scenarioContext[ShortCourseTestKeys.ShortCourseLearner].ToString()!);
        var learnings = dbConnection.GetShortCourseLearningsForLearner(learnerKey);
        var learning = learnings.Single(l => l.TrainingCode == courseCode);
        var episode = dbConnection.GetShortCourseLearning(learning.Key).Episodes.Single();

        episode.IsApproved.Should().BeFalse();
    }

    private static OnProgramme BuildOnProgramme(string courseCode, DateTime? completionDate = null, List<Milestone>? milestones = null) => new()
    {
        Ukprn = ProviderUkprn,
        CourseCode = courseCode,
        StartDate = new DateTime(2025, 1, 1),
        ExpectedEndDate = new DateTime(2025, 6, 30),
        CompletionDate = completionDate,
        EmployerId = 99999999,
        Price = 1000,
        Milestones = milestones ?? new List<Milestone>()
    };

    private static CreateDraftShortCourseRequest BuildRequest(string courseCode, long uln = 123213) => new()
    {
        Ukprn = ProviderUkprn,
        OnProgramme =
        [
            new OnProgramme
            {
                Ukprn = ProviderUkprn,
                CourseCode = courseCode,
                StartDate = new DateTime(2025, 1, 1),
                ExpectedEndDate = new DateTime(2025, 6, 30),
                EmployerId = 99999999,
                Price = 1000,
                Milestones = []
            }
        ],
        LearnerUpdateDetails = new InnerApi.Requests.Apprenticeships.ShortCourseLearnerUpdateDetails
        {
            FirstName = "Frank",
            LastName = "Frankinson",
            DateOfBirth = new DateTime(2000, 1, 1),
            Uln = uln,
            LearnerRef = "LR-123213"
        },
        LearningSupport = []
    };
}
