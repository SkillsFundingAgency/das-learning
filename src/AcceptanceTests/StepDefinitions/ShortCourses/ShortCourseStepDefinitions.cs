using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;
using SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses
{
    [Binding]
    public class ShortCourseStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        private string ShortCourseLearningKey = "ShortCourseLearningKey";
        private string ShortCourseEndpointResponseCodeKey = "ShortCourseEndpointResponseCode";

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

            await CallCreateShortCourseEndpoint(request);
        }

        [Given(@"short course is approved")]
        public async Task GivenShortCourseWithUlnIsApproved()
        {
            var shortCourseLearningKey = new Guid(_scenarioContext[ShortCourseLearningKey].ToString()!);

            await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
            await dbConnection.OpenAsync();

            var sql = @"
                UPDATE ep
                SET ep.IsApproved = 1
                FROM ShortCourseEpisode ep
                WHERE ep.LearningKey = @shortCourseLearningKey";

            await dbConnection.ExecuteAsync(sql, new
            {
                shortCourseLearningKey
            });
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
                    }
                },
                LearnerUpdateDetails = new ShortCourseLearnerUpdateDetails
                {
                    FirstName = "Frank",
                    LastName = "Frankinson",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Uln = 123213
                },
                LearningSupport = new List<LearningSupportDetails>()
            };
        }

        private async Task CallCreateShortCourseEndpoint(CreateDraftShortCourseRequest request)
        {
            (var responseBody, var statusCode) = await _testContext.TestInnerApi.PostWithResponseCode<CreateDraftShortCourseRequest, Guid?>($"/shortCourses", request);
            _scenarioContext[ShortCourseLearningKey] = responseBody;
            _scenarioContext[ShortCourseEndpointResponseCodeKey] = (int)statusCode;
        }

        private async Task<bool> ShortCourseRecordMatchesExpectation()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
            var shortCourses = (await dbConnection.GetAllAsync<DataAccess.Entities.Learning.ShortCourseLearning>()).Where(x => x.Key == Guid.Parse(_scenarioContext[ShortCourseLearningKey].ToString()));
            var shortCourse = shortCourses.SingleOrDefault();

            return shortCourse != null;
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
}
