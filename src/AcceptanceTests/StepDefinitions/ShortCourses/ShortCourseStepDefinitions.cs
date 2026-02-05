using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;
using SFA.DAS.Learning.Enums;
using Microsoft.Data.SqlClient;
using Dapper.Contrib.Extensions;
using SFA.DAS.Learning.InnerApi.Requests.Shared;

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
            var request = new CreateDraftShortCourseRequest
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
                LearnerUpdateDetails = new LearnerUpdateDetails
                {
                    FirstName = "Frank",
                    LastName = "Frankinson",
                    DateOfBirth = new DateTime(2000, 1, 1),
                },
                LearningSupport = new List<LearningSupportDetails>()
            };
            var response = await _testContext.TestInnerApi.Post<CreateDraftShortCourseRequest, Guid>($"/shortCourses", request);
            _scenarioContext[ShortCourseLearningKey] = response;
        }

        [Then("a short course record is created")]
        public async Task AShortCourseRecordIsCreated()
        {
            await WaitHelper.WaitForIt(async () => await ShortCourseRecordMatchesExpectation(), $"Failed to find the short course record");
        }

        private async Task<bool> ShortCourseRecordMatchesExpectation()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
            var shortCourses = (await dbConnection.GetAllAsync<DataAccess.Entities.Learning.ShortCourseLearning>()).Where(x => x.Key == Guid.Parse(_scenarioContext[ShortCourseLearningKey].ToString()));
            var shortCourse = shortCourses.SingleOrDefault();

            return shortCourse != null;
        }
    }
}
