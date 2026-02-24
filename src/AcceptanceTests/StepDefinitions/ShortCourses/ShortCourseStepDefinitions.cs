using Azure.Core;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions.ShortCourses
{
    [Binding]
    public class ShortCourseStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        private string ShortCourseLearningKey = "ShortCourseLearningKey";

        public ShortCourseStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }

        [Given(@"SLD has informed the system that a new short course has been created")]
        public async Task SLDHasInformedTheSystemThatANewShortCourseHasBeenCreated()
        {
            var request = GetDefaultShortCourse();
            await CallCreateShortCourseEndpoint(request);
        }

        [Given(@"A new SLD course with the following information is sent")]
        public async Task SLDHasInformedTheSystemThatANewShortCourseHasBeenCreated(Table table)
        {
            var request = GetDefaultShortCourse();

            var row = table.Rows[0];
            var firstName = row["FirstName"];
            if (!string.IsNullOrWhiteSpace(firstName))
                request.LearnerUpdateDetails.FirstName = firstName;

            var lastName = row["LastName"];
            if (!string.IsNullOrWhiteSpace(lastName))
                request.LearnerUpdateDetails.LastName = lastName;

            if (row.TryGetValue("Uln", out var uln) && long.TryParse(uln, out var parsedUln))
                request.LearnerUpdateDetails.Uln = parsedUln;

            var learningSupportDetails = GetLearningSupportDetails(table);

            foreach (var learningSupport in learningSupportDetails)
            {
                request.LearningSupport.Add(learningSupport);
            }

            await CallCreateShortCourseEndpoint(request);
        }

        [Then("a short course record is created")]
        public async Task AShortCourseRecordIsCreated()
        {
            await WaitHelper.WaitForIt(async () => await ShortCourseRecordMatchesExpectation(), $"Failed to find the short course record");
        }

        [Then(@"a short course record is created with")]
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

            if(row.TryGetValue("LastName", out var lastName))
            {
                learner.LastName.Should().Be(lastName);
            }

            var learningSupportDetails = GetLearningSupportDetails(table);

            foreach(var learningSupport in learningSupportDetails)
            {
                shortCourseLearning.Episodes.First().LearningSupport.Should().Contain(ls => ls.StartDate == learningSupport.StartDate && ls.EndDate == learningSupport.EndDate);
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
            var response = await _testContext.TestInnerApi.Post<CreateDraftShortCourseRequest, Guid>($"/shortCourses", request);
            _scenarioContext[ShortCourseLearningKey] = response;
        }

        private async Task<bool> ShortCourseRecordMatchesExpectation()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
            var shortCourses = (await dbConnection.GetAllAsync<DataAccess.Entities.Learning.ShortCourseLearning>()).Where(x => x.Key == Guid.Parse(_scenarioContext[ShortCourseLearningKey].ToString()));
            var shortCourse = shortCourses.SingleOrDefault();

            return shortCourse != null;
        }

        private static List<LearningSupportDetails> GetLearningSupportDetails(Table table)
        {
            var learningSupportDetails = new List<LearningSupportDetails>();
            var learningSupportStrings = table.GetIndexedListValues("LearningSupport");

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
