using AutoFixture;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class UpdateLearnerStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;

    public UpdateLearnerStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
    }

    [Given(@"an update request has the following data")]
    public void ConfigureUpdateRequest(Table table)
    {
        var updateRequest = _scenarioContext.GetUpdateLearnerRequest();

        foreach (var row in table.Rows)
        {
            var propertyName = row["Property"];
            var valueString = row["Value"];

            switch(propertyName)
            {
                case "CompletionDate":
                    updateRequest.Learner.CompletionDate = TokenisableDateTime.FromString(valueString).DateTime;
                    break;
                case "WithdrawalDate":
                    updateRequest.Delivery.WithdrawalDate = TokenisableDateTime.FromString(valueString).DateTime;
                    break;
                case "MathsAndEnglish":
                    updateRequest.MathsAndEnglishCourses = GetMathsAndEnglishFromString(valueString);
                    break;
                case "LearningSupport":
                    updateRequest.LearningSupport = GetLearningSupportFromString(valueString);
                    break;
                case "Prices":
                    updateRequest.OnProgramme.Costs = GetCostsFromString(valueString);
                    break;
                case "ExpectedEndDate":
                    updateRequest.OnProgramme.ExpectedEndDate = TokenisableDateTime.FromString(valueString).DateTime!.Value;
                    break;
                default:
                    throw new ArgumentException($"Property '{propertyName}' is not recognized.");
            }
        }
    }

    [Given(@"the update request is sent")]
    [When(@"the update request is sent")]
    public async Task WhenTheUpdateRequestIsSent()
    {
        var updateRequest = _scenarioContext.GetUpdateLearnerRequest();
        var learningKey = _scenarioContext.GetLearningKey();
        var updateResponse =  await _testContext.TestInnerApi.Put<UpdateLearnerRequest, UpdateLearnerResult>($"/{learningKey}", updateRequest);
        _scenarioContext.SetUpdateLearnerResult(updateResponse);
    }

    [When(@"an update request is sent again with the same data")]
    public async Task WhenAnUpdateRequestIsSentWithTheSameData()
    {
        await Task.Delay(1000); // Ensure any previous events have been processed
        _testContext.MessageSession.ClearEventsOfType<LearningWithdrawnEvent>();

        await WhenTheUpdateRequestIsSent();
    }

    [Then(@"the “last day of learning” for the Learning is set to (.*)")]
    public async Task ThenTheLastDayOfLearningForTheLearningIsSetToTheProvidedWithdrawalDate(TokenisableDateTime withdrawDate)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        learning.Episodes.First().LastDayOfLearning = withdrawDate.DateTime;
    }

    [Then(@"the Earnings for the Learning are recalculated")]
    public void ThenTheEarningsForTheLearningAreRecalculated()
    {
        _scenarioContext.Pending();
    }

    [Then(@"the following changes are returned")]
    public void ValidateChangesReturned(Table table)
    {
        var changes = _scenarioContext.GetUpdateLearnerResult().Changes;

        var expectedChanges = table.Rows
            .Select(row => Enum.Parse<LearningUpdateChanges>(row["Change"]))
            .ToList();

        changes.Should().BeEquivalentTo(expectedChanges,
            "the returned changes should exactly match the expected changes");
    }

    [Then(@"the EpisodePrices are returned")]
    public void ThenTheEpisodePricesAreReturned(Table table)
    {
        var episodePrices = _scenarioContext.GetUpdateLearnerResult().Prices;

        var expectedPrices = table.Rows
            .Select(row => new SFA.DAS.Learning.Command.UpdateLearner.UpdateLearnerResult.EpisodePrice
            {
                StartDate = TokenisableDateTime.FromString(row["StartDate"]).DateTime!.Value,
                EndDate = TokenisableDateTime.FromString(row["EndDate"]).DateTime!.Value,
                TrainingPrice = int.Parse(row["TrainingPrice"]),
                EndPointAssessmentPrice = int.Parse(row["EpaPrice"])
            })
            .ToList();

        foreach (var expectedPrice in expectedPrices)
        {
            episodePrices.Should().ContainEquivalentOf(expectedPrice, options => options
                .Excluding(c => c.Key)
                .Excluding(c => c.TotalPrice)
                .Excluding(c => c.FundingBandMaximum));
        }

    }

    [Then(@"the Completion Date for the Learning is set to (.*)")]
    public async Task ThenTheCompletionDateForTheLearningIsSetToCurrentAY(TokenisableDateTime completionDate)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        learning.CompletionDate = completionDate.DateTime;
    }

    [Then(@"the following maths and english details are stored")]
    public async Task ThenTheFollowingMathsAndEnglishDetailsAreStored(Table table)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);

        learning.MathsAndEnglishCourses.Should().HaveCount(table.Rows.Count, "the number of stored maths and english records should match the expected count");

        foreach (var row in table.Rows)
        {
            var expectedMathsAndEnglish = new DataAccess.Entities.Learning.MathsAndEnglish
            {
                Course = row["Course"],
                StartDate = TokenisableDateTime.FromString(row["StartDate"]).DateTime!.Value,
                PlannedEndDate = TokenisableDateTime.FromString(row["PlannedEndDate"]).DateTime!.Value,
                Amount = decimal.Parse(row["Amount"])
            };

            learning.MathsAndEnglishCourses
                .Should().ContainEquivalentOf(expectedMathsAndEnglish, options => options
                    .Excluding(c => c.CompletionDate)
                    .Excluding(c => c.WithdrawalDate)
                    .Excluding(c => c.PriorLearningPercentage)
                    .Excluding(c => c.LearningKey)
                    .Excluding(c => c.Key)
                    .Using<string>(ctx => ctx.Subject?.Trim().Should().Be(ctx.Expectation?.Trim()))
                    .WhenTypeIs<string>());
        }
    }

    [Then(@"the following LearningSupport details are stored")]
    public async Task ThenTheFollowingLearningSupportDetailsAreStored(Table table)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        var episode = learning.Episodes.Single();
        episode.LearningSupport.Should().HaveCount(table.Rows.Count, "the number of stored learning support records should match the expected count");

        foreach (var row in table.Rows)
        {
            var expectedLearningSupport = new DataAccess.Entities.Learning.LearningSupport
            {
                StartDate = TokenisableDateTime.FromString(row["StartDate"]).DateTime!.Value,
                EndDate = TokenisableDateTime.FromString(row["EndDate"]).DateTime!.Value
            };

            episode.LearningSupport.Should().ContainEquivalentOf(expectedLearningSupport, options => options
                    .Excluding(c => c.LearningKey)
                    .Excluding(c => c.EpisodeKey)
                    .Excluding(c => c.Key));
        }
    }

    [Then(@"the following Price details are stored")]
    public async Task ThenTheFollowingPriceDetailsAreStored(Table table)
    {
        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learning = dbConnection.GetLearning(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        var episodePrices = learning.Episodes.Single().Prices;
        episodePrices.Should().HaveCount(table.Rows.Count, "the number of stored episodePrice records should match the expected count");

        foreach (var row in table.Rows)
        {
            var price = new DataAccess.Entities.Learning.EpisodePrice
            {
                StartDate = TokenisableDateTime.FromString(row["StartDate"]).DateTime!.Value,
                EndDate = TokenisableDateTime.FromString(row["EndDate"]).DateTime!.Value,
                EndPointAssessmentPrice = int.Parse(row["EpaPrice"]),
                TrainingPrice = int.Parse(row["TrainingPrice"])
            };

            episodePrices.Should().ContainEquivalentOf(price, options => options
                    .Excluding(c => c.EpisodeKey)
                    .Excluding(c => c.Key)
                    .Excluding(c => c.TotalPrice)
                    .Excluding(c => c.FundingBandMaximum));
        }
    }

    private List<MathsAndEnglish> GetMathsAndEnglishFromString(string valueString)
    {
        var parsedValues = KeyValueParser.Parse(valueString);
        var courses = new List<MathsAndEnglish>();

        if (parsedValues.Any())
        {
            courses.Add(new MathsAndEnglish
            {
                Course = parsedValues.GetValueOrDefault("course", "Maths"),
                StartDate = TokenisableDateTime.FromString(parsedValues["StartDate"]).DateTime!.Value,
                PlannedEndDate = TokenisableDateTime.FromString(parsedValues["PlannedEndDate"]).DateTime!.Value,
                Amount = decimal.Parse(parsedValues.GetValueOrDefault("Amount", "1000")),
                WithdrawalDate = parsedValues.TryGetValue("WithdrawalDate", out var parsedWithdrawalDate)
                    ? TokenisableDateTime.FromString(parsedWithdrawalDate).DateTime!.Value
                    : null
            });
        }

        return courses;
    }

    private List<LearningSupportUpdatedDetails> GetLearningSupportFromString(string valueString)
    {
        var parsedValues = KeyValueParser.Parse(valueString);
        var learningSupport = new List<LearningSupportUpdatedDetails>();

        if (parsedValues.Any())
        {
            learningSupport.Add(new LearningSupportUpdatedDetails
            {
                StartDate = TokenisableDateTime.FromString(parsedValues["StartDate"]).DateTime!.Value,
                EndDate = TokenisableDateTime.FromString(parsedValues["EndDate"]).DateTime!.Value
            });
        }

        return learningSupport;
    }

    private List<Cost> GetCostsFromString(string valueString)
    {
        var parsedValues = KeyValueParser.Parse(valueString);
        var costs = new List<Cost>();

        if (parsedValues.Any())
        {
            costs.Add(new Cost
            {
                FromDate = TokenisableDateTime.FromString(parsedValues["fromDate"]).DateTime!.Value,
                TrainingPrice = int.Parse(parsedValues["trainingPrice"]),
                EpaoPrice = int.Parse(parsedValues["epaoPrice"])
            });
        }

        return costs;
    }

    private static class KeyValueParser
    {
        public static Dictionary<string, string> Parse(string input)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var pairs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var parts = pair.Split(':', 2); // split into key and value, max 2 parts
                if (parts.Length == 2)
                {
                    result[parts[0]] = parts[1];
                }
            }

            return result;
        }
    }
}
