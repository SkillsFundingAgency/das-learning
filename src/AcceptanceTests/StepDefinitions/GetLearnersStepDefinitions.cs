using AutoFixture;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class GetLearnersStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;
    private readonly LearningDataSeeder _learningDataSeeder;
    private const string _LearnersByCategoryKey = "LearnersByCategory";
    private const string _ActiveLearnersResponseKey = "ActiveLearnersResponse";

    public GetLearnersStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
        _learningDataSeeder = new LearningDataSeeder(_scenarioContext, _testContext, _fixture);
    }

    [Given(@"a provider has learners")]
    public async Task GivenAProviderHasLearners(Table table)
    {
        var learnersByCategory = new Dictionary<string, List<string>>();
        var totalCreated = 0;

        foreach (var row in table.Rows)
        {
            var numberToCreate = int.Parse(row["Number"]);
            var category = row["Category"];
            List<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>? createdLearners = null;

            switch (category)
            {
                case "CurrentlyActive":
                    createdLearners = await CreateLearners(numberToCreate, "currentAY-09-01", "nextAY-07-31");
                    break;
                case "CompletedInPreviousYear":
                    createdLearners = await CreateLearners(numberToCreate, "TwoYearsAgoAY-09-01", "previousAY-07-31");
                    break;
                case "StartNextYear":
                    createdLearners = await CreateLearners(numberToCreate, "nextAY-09-01", "currentAYPlusTwo-07-31");
                    break;
                case "WithdrawnInPreviousYear":
                    createdLearners = await CreateWithdrawnLearners(numberToCreate, "TwoYearsAgoAY-09-01", "previousAY-07-31", "previousAY-03-31");
                    break;
                case "WithdrawnToStartInThisYear":
                    createdLearners = await CreateWithdrawnLearners(numberToCreate, "currentAY-09-01", "nextAY-07-31", "currentAY-09-01");
                    break;
                case "WithdrawnInThisYear":
                    createdLearners = await CreateWithdrawnLearners(numberToCreate, "currentAY-09-01", "nextAY-07-31", "currentAY-11-01");
                    break;
            }

            totalCreated += numberToCreate;
            learnersByCategory.Add(category, createdLearners!.Select(l => l.Uln).ToList());
        }

        if(totalCreated > 20)
        {
            throw new Exception("If more than 20 are added then the get request should be updated as paging by default uses a page size of 20");
        }

        _scenarioContext.Set(learnersByCategory, _LearnersByCategoryKey);

    }

    [When(@"SLD requests the list of active Learnings for the provider in an academic year")]
    public async Task WhenSLDRequestsTheListOfActiveLearningsForTheProviderInAnAcademicYear()
    {
        int startYearOfCurrentAcademicYear = System.DateTime.Now.Month > 7 ? System.DateTime.Now.Year : System.DateTime.Now.Year - 1;
        var academicYear = $"{startYearOfCurrentAcademicYear % 100:D2}{(startYearOfCurrentAcademicYear + 1) % 100:D2}";

        var learners = await _testContext.TestInnerApi.Get<GetLearningsByAcademicYearResponse>($"{Constants.UkPrn}/academicyears/{academicYear}/learnings");
        _scenarioContext.Set(learners, _ActiveLearnersResponseKey);
    }

    [Then(@"the list of Learnings sent does not include any Learnings where the Learning was active in that year and has been been withdrawn back to the Learning's start date")]
    public void NoLearnersInCurrentYearWithdrawnToStartReturned()
    {
        AssertLearnersInCategoryNotReturned("WithdrawnToStartInThisYear");
    }

    [Then(@"the list of Learnings sent does not include any Learnings where the Learning was withdrawn in a prior year")]
    public void NoLearnersWithdrawnInPreviousYearReturned()
    {
        AssertLearnersInCategoryNotReturned("WithdrawnInPreviousYear");
    }

    [Then(@"the list of Learnings sent includes any Learnings where the Learning was active in that year but have now been withdrawn while in-learning")]
    public void IncludesLearnersWithdrawnInCurrentYear()
    {
        AssertLearnersInCategoryAreReturned("WithdrawnInThisYear");
    }

    [Then(@"the list of Learnings sent includes any Learnings where the Learning is or was active in that year")]
    public void ThenTheListOfLearningsSentIncludesAnyLearningsWhereTheLearningIsOrWasActiveInThatYear()
    {
        AssertLearnersInCategoryAreReturned("CurrentlyActive");
    }

    private async Task<List<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>> CreateWithdrawnLearners(int numberToCreate, string startDate, string endDate, string withdrawnDate)
    {
        var createdEvents = await CreateLearners(numberToCreate, startDate, endDate);

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());

        foreach (var createdEvent in createdEvents)
        {
            var learningKey = dbConnection.GetLearningKey(createdEvent.Uln);
            var updateRequest = createdEvent.BuildUpdateLearnerRequest();

            updateRequest.Delivery.WithdrawalDate = TokenisableDateTime.FromString(withdrawnDate).DateTime!.Value;
            var updateResponse = await _testContext.TestInnerApi.Put<UpdateLearnerRequest, UpdateLearnerResult>($"/{learningKey}", updateRequest);
            
            if (!updateResponse.Changes.Contains(LearningUpdateChanges.Withdrawal))
            {
                throw new Exception($"Failed to withdraw learner with Uln {createdEvent.Uln}");
            }
        }

        return createdEvents;
    }

    private async Task<List<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>> CreateLearners(int numberToCreate, string startDate, string endDate)
    {
        var createdEvents = new List<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>();

        for (int i = 0; i < numberToCreate; i++)
        {
            var createdEvent = await _learningDataSeeder.CreateLearner(
                actualStartDate: TokenisableDateTime.FromString(startDate).DateTime!.Value,
                endDate: TokenisableDateTime.FromString(endDate).DateTime!.Value,
                trainingPrice: 6000,
                epaPrice: 500);

            createdEvents.Add(createdEvent);
        }

        return createdEvents;
    }

    private void AssertLearnersInCategoryNotReturned(string category)
    {
        var learnersByCategory = _scenarioContext.Get<Dictionary<string, List<string>>>(_LearnersByCategoryKey);
        var activeLearnersResponse = _scenarioContext.Get<GetLearningsByAcademicYearResponse>(_ActiveLearnersResponseKey);
        var learnersNotExpected = learnersByCategory[category];

        activeLearnersResponse.Items.Should().NotContain(learner => learnersNotExpected.Contains(learner.Uln));

    }

    private void AssertLearnersInCategoryAreReturned(string category)
    {
        var learnersByCategory = _scenarioContext.Get<Dictionary<string, List<string>>>(_LearnersByCategoryKey);
        var activeLearnersResponse = _scenarioContext.Get<GetLearningsByAcademicYearResponse>(_ActiveLearnersResponseKey);
        var learnersExpected = learnersByCategory[category];

        activeLearnersResponse.Items.Should().Contain(learner => learnersExpected.Contains(learner.Uln));
    }
}
