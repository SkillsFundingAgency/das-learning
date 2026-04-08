using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;
using SFA.DAS.Learning.InnerApi.Responses;
using SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;
using SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses;

[Binding]
public class ShortCourseStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    private string ShortCourseLearningKey = "ShortCourseLearningKey";
    private string ShortCourseEndpointResponseCodeKey = "ShortCourseEndpointResponseCode";
    private string UpdateShortCourseResultKey = "UpdateShortCourseResult";

    public ShortCourseStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"SLD has informed the system of the following short courses")]
    public async Task SLDHasInformedTheSystemOfTheFollowingShortCourses(Table table)
    {
        foreach (var tableRow in table.Rows)
        {
            await CallShortCourseEndpointFromTableRow(tableRow);
        }
    }

    [Given(@"SLD has informed the system that a new short course has been created")]
    public async Task SLDHasInformedTheSystemThatANewShortCourseHasBeenCreated()
    {
        var request = GetDefaultShortCourse();
        await CallCreateShortCourseEndpoint(request);
    }

    [Given(@"SLD call the create short course endpoint with the following information")]
    public async Task SLDHasInformedTheSystemThatANewShortCourseHasBeenCreated(Table table)
    {
        var row = table.Rows[0];

        await CallShortCourseEndpointFromTableRow(row);
    }

    private async Task CallShortCourseEndpointFromTableRow(TableRow row)
    {
        var request = GetDefaultShortCourse();
        request.OnProgramme.WithdrawalDate = null;

        if (row.TryGetValue("Ukprn", out var ukprn) && long.TryParse(ukprn, out var parsedUkprn))
            request.OnProgramme.Ukprn = parsedUkprn;

        if (row.TryGetValue("FirstName", out var firstName))
            request.LearnerUpdateDetails.FirstName = firstName;

        if (row.TryGetValue("LastName", out var lastName))
            request.LearnerUpdateDetails.LastName = lastName;

        if (row.TryGetValue("Uln", out var uln) && long.TryParse(uln, out var parsedUln))
            request.LearnerUpdateDetails.Uln = parsedUln;

        if (row.TryGetValue("StartDate", out var startDate) && DateTime.TryParse(startDate, out var parsedStartDate))
            request.OnProgramme.StartDate = parsedStartDate;

        if (row.TryGetValue("ExpectedEndDate", out var expectedEndDate) && DateTime.TryParse(expectedEndDate, out var parsedExpectedEndDate))
            request.OnProgramme.ExpectedEndDate = parsedExpectedEndDate;

        if(row.TryGetValue("Milestone", out var milestone))
        {
            if (Enum.TryParse<Milestone>(milestone, out var parsedMilestone))
            {
                request.OnProgramme.Milestones = new List<Milestone> { parsedMilestone };
            }
        }

        var learningSupportDetails = GetLearningSupportDetails(row);

        foreach (var learningSupport in learningSupportDetails)
        {
            request.LearningSupport.Add(learningSupport);
        }

        if (row.TryGetValue("Price", out var price) && decimal.TryParse(price, out var parsedPrice))
            request.OnProgramme.Price = parsedPrice;

        if (row.TryGetValue("WithdrawalDate", out var withdrawalDate) && DateTime.TryParse(withdrawalDate, out var parsedWithdrawalDate))
            request.OnProgramme.WithdrawalDate = parsedWithdrawalDate;

        if (row.TryGetValue("CompletionDate", out var completionDate) && DateTime.TryParse(completionDate, out var parsedCompletionDate))
            request.OnProgramme.CompletionDate = parsedCompletionDate;

        var learningKey = await CallCreateShortCourseEndpoint(request);

        if(row.TryGetValue("IsApproved", out var isApproved) && isApproved.ToLower() == "true")
        {
            byte employerType = 0;
            if (row.TryGetValue("EmployerType", out var employerTypeStr) && Enum.TryParse<EmployerType>(employerTypeStr, out var parsedEmployerType))
                employerType = (byte)parsedEmployerType;
            await ApproveCourseInDb(learningKey, employerType);
        }
    }

    private async Task ApproveCourseInDb(Guid learningKey, byte employerType = 0)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        dbConnection.SetAllEpisodesForShortCourseToApproved(learningKey, employerType);
    }

    [Given(@"short course is approved")]
    public async Task GivenShortCourseWithUlnIsApproved()
    {
        var shortCourseLearningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);

        await ApproveCourseInDb(shortCourseLearningKey);
    }

    [When("SLD requests the list of short courses for academic year (.*)")]
    public async Task WhenSLDRequestsTheListOfShortCoursesForAcademicYear(int academicYear)
    {
        var ukprn = GetDefaultShortCourse().OnProgramme.Ukprn;
        var response = await _testContext.TestInnerApi.Get<GetShortCoursesByAcademicYearResponse>($"/{ukprn}/academicyears/{academicYear}/shortCourses");
        _scenarioContext.Set<GetShortCoursesByAcademicYearResponse>(response);
    }

    [Then("a short course record is created")]
    public async Task AShortCourseRecordIsCreated()
    {
        await WaitHelper.WaitForIt(async () => await ShortCourseRecordMatchesExpectation(), $"Failed to find the short course record");
    }

    [Then(@"a short course record is created with")]
    [Then(@"a short course record exists with")]
    public async Task ThenAShortCourseRecordIsCreatedWith(Table table)
    {
        var row = table.Rows[0];
        var shortCourseLearningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learner = dbConnection.GetLearnerByShortCourseKey(shortCourseLearningKey);
        var shortCourseLearning = dbConnection.GetShortCourseLearning(shortCourseLearningKey);

        if (row.TryGetValue("FirstName", out var firstName))
        {
            learner.FirstName.Should().Be(firstName);
        }

        if (row.TryGetValue("LastName", out var lastName))
        {
            learner.LastName.Should().Be(lastName);
        }

        var learningSupportDetails = GetLearningSupportDetails(row);

        foreach (var learningSupport in learningSupportDetails)
        {
            shortCourseLearning.Episodes.First().LearningSupport.Should().Contain(ls => ls.StartDate == learningSupport.StartDate && ls.EndDate == learningSupport.EndDate);
        }

        if (row.TryGetValue("Milestone", out var milestone))
        {
            shortCourseLearning.Episodes.First().Milestones.Should().Contain(m => m.Milestone.ToString() == milestone);
        }
    }

    [Then(@"the response from the create short course endpoint is (.*)")]
    public void ThenTheResponseFromTheCreateShortCourseEndpointIs(string statusCode)
    {
        var expectedStatusCode = _scenarioContext[ShortCourseEndpointResponseCodeKey].ToString();
        expectedStatusCode.Should().Be(statusCode);
    }

    [Then(@"for learner with Uln (.*) there is (.*) short course record")]
    public async Task ThenForLearnerWithUlnThereIsOnlyShortCourseRecord(string uln, int numberOfRecords)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learner = dbConnection.GetLearner(uln);
        var shortCourseLearnings = dbConnection.GetShortCourseLearningsForLearner(learner.Key);

        shortCourseLearnings.Count().Should().Be(numberOfRecords);
    }

    [When("SLD requests short courses for earnings for collection year (.*)")]
    public async Task WhenSLDRequestsShortCoursesForEarningsForCollectionYear(int collectionYear)
    {
        var ukprn = GetDefaultShortCourse().OnProgramme.Ukprn;
        var response = await _testContext.TestInnerApi.Get<GetShortCoursesForEarningsResponse>($"/{ukprn}/{collectionYear}/shortCourses");
        _scenarioContext.Set<GetShortCoursesForEarningsResponse>(response);
    }

    [Then(@"short courses for earnings are returned with the following details")]
    public void ThenShortCoursesForEarningsAreReturnedWithTheFollowingDetails(Table table)
    {
        var response = _scenarioContext.Get<GetShortCoursesForEarningsResponse>();
        response.Items.Count().Should().Be(table.RowCount);
        foreach (var row in table.Rows)
        {
            var item = response.Items.Single(i => i.Learner.Uln == row["Uln"]);
            item.Learner.FirstName.Should().Be(row["FirstName"]);
            item.Learner.LastName.Should().Be(row["LastName"]);
            var episode = item.Episodes.First();
            episode.CourseCode.Should().Be(row["CourseCode"]);
            episode.IsApproved.Should().Be(bool.Parse(row["IsApproved"]));
            if (row.TryGetValue("Price", out var price))
                episode.Price.Should().Be(decimal.Parse(price));
        }
    }

    [Then(@"short courses are returned for the following Ulns")]
    public void ThenShortCoursesAreReturnedForTheFollowingUlns(Table table)
    {
        var response = _scenarioContext.Get<GetShortCoursesByAcademicYearResponse>();
        response.Items.Count().Should().Be(table.RowCount);
        foreach (var row in table.Rows)
        {
            response.Items.Should().Contain(i => i.Uln == row["Uln"]);
        }
    }

    [Then(@"the Short Course is approved")]
    public async Task ThenTheShortCourseIsApproved()
    {
        var shortCourseLearningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);
        var approvalEvent = _scenarioContext.GetApprenticeshipCreatedEvent();

        await WaitHelper.WaitForIt(async () =>
        {
            await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
            var shortCourseLearning = dbConnection.GetShortCourseLearning(shortCourseLearningKey);
            return shortCourseLearning.Episodes.First().IsApproved;
        }, "Short course was not approved in time");

        await using var conn = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = conn.GetShortCourseLearning(shortCourseLearningKey);
        var episode = learning.Episodes.First();

        episode.IsApproved.Should().BeTrue();
        episode.EmployerType.Should().Be(approvalEvent.ApprenticeshipEmployerTypeOnApproval == CommitmentsV2.Types.ApprenticeshipEmployerType.Levy
            ? EmployerType.Levy
            : EmployerType.NonLevy);
    }


    private CreateDraftShortCourseRequest GetDefaultShortCourse()
    {
        return new CreateDraftShortCourseRequest
        {
            OnProgramme = new OnProgramme
            {
                WithdrawalDate = new DateTime(2024, 7, 1),
                ExpectedEndDate = new DateTime(2024, 12, 1),
                StartDate = new DateTime(2024, 1, 1),
                CompletionDate = null,
                Ukprn = 10005001,
                EmployerId = 99999999,
                CourseCode = "SC-ART1",
                Milestones = new List<Milestone>
                {
                    Milestone.ThirtyPercentLearningComplete
                },
                Price = 1000
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

    private async Task<Guid> CallCreateShortCourseEndpoint(CreateDraftShortCourseRequest request)
    {
        (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, CreateShortCourseLearningResponse?>($"/shortCourses", request);

        var learningKey = responseBody?.LearningKey ?? Guid.Empty;

        _scenarioContext[ShortCourseLearningKey] = learningKey;
        _scenarioContext[ShortCourseEndpointResponseCodeKey] = (int)statusCode;

        return learningKey;
    }

    private async Task<bool> ShortCourseRecordMatchesExpectation()
    {
        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
        var shortCourses = (await dbConnection.GetAllAsync<DataAccess.Entities.Learning.ShortCourseLearning>()).Where(x => x.Key == Guid.Parse(_scenarioContext[ShortCourseLearningKey].ToString()));
        var shortCourse = shortCourses.SingleOrDefault();

        return shortCourse != null;
    }

    [When(@"SLD calls the update short course endpoint with the following information")]
    public async Task WhenSLDCallsTheUpdateShortCourseEndpointWithTheFollowingInformation(Table table)
    {
        var row = table.Rows[0];
        var request = GetDefaultShortCourse();

        if (row.TryGetValue("WithdrawalDate", out var withdrawalDate) && DateTime.TryParse(withdrawalDate, out var parsedWithdrawalDate))
            request.OnProgramme.WithdrawalDate = parsedWithdrawalDate;

        if (row.TryGetValue("CompletionDate", out var completionDate) && DateTime.TryParse(completionDate, out var parsedCompletionDate))
            request.OnProgramme.CompletionDate = parsedCompletionDate;

        if (row.TryGetValue("Milestone", out var milestone) && Enum.TryParse<Milestone>(milestone, out var parsedMilestone))
            request.OnProgramme.Milestones = new List<Milestone> { parsedMilestone };

        await CallUpdateShortCourseEndpoint(request);
    }

    [When(@"SLD delete the short course")]
    public async Task WhenSLDCallsTheDeleteShortCourseEndpoint()
    {
        var learningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);
        var ukprn = GetDefaultShortCourse().OnProgramme.Ukprn;
        await _testContext.TestInnerApi.Delete($"/{ukprn}/shortCourses/{learningKey}");
    }

    [Then(@"the short course episode WithdrawalDate equals the StartDate")]
    public async Task ThenTheShortCourseEpisodeWithdrawalDateEqualsTheStartDate()
    {
        var shortCourseLearningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var shortCourse = dbConnection.GetShortCourseLearning(shortCourseLearningKey);
        var episode = shortCourse.Episodes.Single();
        episode.WithdrawalDate.Should().Be(episode.StartDate);
    }

    [When(@"SLD calls the update short course endpoint with no changes")]
    public async Task WhenSLDCallsTheUpdateShortCourseEndpointWithNoChanges()
    {
        await CallUpdateShortCourseEndpoint(GetDefaultShortCourse());
    }

    [Then(@"the update short course response includes changes")]
    public void ThenTheUpdateShortCourseResponseIncludesChanges(Table table)
    {
        var result = (UpdateShortCourseTestResult)_scenarioContext[UpdateShortCourseResultKey];
        var expectedChanges = table.Rows.Select(r => r["Change"]).ToList();

        result.Changes.Should().BeEquivalentTo(expectedChanges);
    }

    [Then(@"the update short course response includes no changes")]
    public void ThenTheUpdateShortCourseResponseIncludesNoChanges()
    {
        var result = (UpdateShortCourseTestResult)_scenarioContext[UpdateShortCourseResultKey];
        result.Changes.Should().BeEmpty();
    }

    [Then(@"the update short course response has a completion date of (.*)")]
    public void ThenTheUpdateShortCourseResponseHasACompletionDateOf(DateTime completionDate)
    {
        var result = (UpdateShortCourseTestResult)_scenarioContext[UpdateShortCourseResultKey];
        result.CompletionDate.Should().Be(completionDate);
    }

    [Then(@"the update short course response includes the following learner details")]
    public void ThenTheUpdateShortCourseResponseIncludesTheFollowingLearnerDetails(Table table)
    {
        var result = (UpdateShortCourseTestResult)_scenarioContext[UpdateShortCourseResultKey];
        var row = table.Rows[0];

        result.Learner.Should().NotBeNull();

        if (row.TryGetValue("Uln", out var uln))
            result.Learner.Uln.Should().Be(uln);

        if (row.TryGetValue("FirstName", out var firstName))
            result.Learner.FirstName.Should().Be(firstName);

        if (row.TryGetValue("LastName", out var lastName))
            result.Learner.LastName.Should().Be(lastName);
    }

    [Then(@"the update short course response includes the following episode details")]
    public void ThenTheUpdateShortCourseResponseIncludesTheFollowingEpisodeDetails(Table table)
    {
        var result = (UpdateShortCourseTestResult)_scenarioContext[UpdateShortCourseResultKey];
        var row = table.Rows[0];

        result.Episodes.Should().NotBeEmpty();
        var episode = result.Episodes.First();

        if (row.TryGetValue("Ukprn", out var ukprn) && long.TryParse(ukprn, out var parsedUkprn))
            episode.Ukprn.Should().Be(parsedUkprn);

        if (row.TryGetValue("EmployerAccountId", out var employerAccountId) && long.TryParse(employerAccountId, out var parsedEmployerAccountId))
            episode.EmployerAccountId.Should().Be(parsedEmployerAccountId);

        if (row.TryGetValue("CourseCode", out var courseCode))
            episode.CourseCode.Should().Be(courseCode);

        if (row.TryGetValue("CourseType", out var courseType))
            episode.CourseType.Should().Be(courseType);

        if (row.TryGetValue("IsApproved", out var isApproved) && bool.TryParse(isApproved, out var parsedIsApproved))
            episode.IsApproved.Should().Be(parsedIsApproved);

        if (row.TryGetValue("Price", out var price) && decimal.TryParse(price, out var parsedPrice))
            episode.Price.Should().Be(parsedPrice);

        if (row.TryGetValue("AgeAtStart", out var ageAtStart) && int.TryParse(ageAtStart, out var parsedAgeAtStart))
            episode.AgeAtStart.Should().Be(parsedAgeAtStart);
    }

    private async Task CallUpdateShortCourseEndpoint(CreateDraftShortCourseRequest request)
    {
        var learningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);
        var result = await _testContext.TestInnerApi.Put<CreateDraftShortCourseRequest, UpdateShortCourseTestResult>($"/shortCourses/{learningKey}", request);
        _scenarioContext[UpdateShortCourseResultKey] = result;
    }

    private class UpdateShortCourseTestResult
    {
        public Guid LearningKey { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string[] Changes { get; set; } = [];
        public UpdateShortCourseTestResultLearner Learner { get; set; } = null!;
        public UpdateShortCourseTestResultEpisode[] Episodes { get; set; } = [];
    }

    private class UpdateShortCourseTestResultLearner
    {
        public string Uln { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    private class UpdateShortCourseTestResultEpisode
    {
        public long Ukprn { get; set; }
        public long EmployerAccountId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseType { get; set; } = null!;
        public bool IsApproved { get; set; }
        public decimal Price { get; set; }
        public int AgeAtStart { get; set; }
    }

    private static List<LearningSupportDetails> GetLearningSupportDetails(TableRow row)
    {
        var learningSupportDetails = new List<LearningSupportDetails>();
        var learningSupportStrings = row.GetIndexedListValues("LearningSupport");

        foreach (var learningSupportString in learningSupportStrings)
        {
            var startDate = learningSupportString.GetPropertyValue("startDate");
            var endDate = learningSupportString.GetPropertyValue("endDate");
            learningSupportDetails.Add(new LearningSupportDetails
            {
                StartDate = TokenisableDateTime.FromString(startDate!).DateTime!.Value,
                EndDate = TokenisableDateTime.FromString(endDate!).DateTime!.Value
            });
        }

        return learningSupportDetails;
    }
}
