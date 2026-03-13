using AutoFixture;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Types;
using System.Data.Common;
using SFA.DAS.CommitmentsV2.Messages.Events;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class ApprovalCreatedStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private readonly Fixture _fixture;
    private readonly LearningDataSeeder _learningDataSeeder;

    public ApprovalCreatedStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        _fixture = new Fixture();
        _learningDataSeeder = new LearningDataSeeder(_scenarioContext, _testContext, _fixture);
    }

    [Given(@"An apprenticeship has been created as part of the approvals journey")]
    [Given(@"that an apprentice record has been approved by an employer previously")]
    public async Task GivenAnApprenticeshipHasBeenCreatedAsPartOfTheApprovalsJourney()
    {

        var approvalCreatedEvent = _fixture.Build<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>()
            .With(_ => _.TrainingCourseVersion, "1.0")
            .With(_ => _.IsOnFlexiPaymentPilot, true)
            .With(_ => _.Uln, _fixture.Create<long>().ToString)
            .With(_ => _.TrainingCode, _fixture.Create<int>().ToString)
            .With(_ => _.PriceEpisodes, new CommitmentsV2.Messages.Events.PriceEpisode[] {
                new CommitmentsV2.Messages.Events.PriceEpisode
                {
                    Cost = 6500,
                    FromDate = TokenisableDateTime.FromString("currentAY-09-25").DateTime!.Value,
                    ToDate = TokenisableDateTime.FromString("nextAY-07-31").DateTime,
                    EndPointAssessmentPrice = 500,
                    TrainingPrice = 6000
                }
            })
            .Create();

        await _testContext.TestFunction!.PublishEvent(approvalCreatedEvent);

        _scenarioContext.SetApprenticeshipCreatedEvent(approvalCreatedEvent);
    }

    [When(@"the Short Course has been approved by an employer")]
    public async Task GivenTheShortCourseHasBeenApprovedByAnEmployer()
    {
        var approvalCreatedEvent = _fixture.Build<ApprenticeshipCreatedEvent>()
            .With(_ => _.Uln, "123213")
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        await _testContext.TestFunction!.PublishEvent(approvalCreatedEvent);

        _scenarioContext.SetApprenticeshipCreatedEvent(approvalCreatedEvent);
    }

    [Given(@"There is an apprenticeship with the following details")]
    public async Task GivenAnApprenticeshipHasBeenCreatedAsPartOfTheApprovalsJourney(Table table)
    {
        var row = table.Rows[0];
        var endDate = TokenisableDateTime.FromString(row["EndDate"]).DateTime!.Value;
        var trainingPrice = decimal.Parse(row["TrainingPrice"]);
        var epaPrice = decimal.Parse(row["EpaPrice"]);

        DateTime? actualStartDate = null;
        if (row.ContainsKey("StartDate") && !string.IsNullOrWhiteSpace(row["StartDate"]))
            actualStartDate = TokenisableDateTime.FromString(row["StartDate"]).DateTime!.Value;

        DateTime? plannedStartDate = null;
        if (row.ContainsKey("PlannedStartDate") && !string.IsNullOrWhiteSpace(row["PlannedStartDate"]))
            plannedStartDate = TokenisableDateTime.FromString(row["PlannedStartDate"]).DateTime!.Value;

        string? uln = null;
        if (row.ContainsKey("Uln") && !string.IsNullOrWhiteSpace(row["Uln"]))
            uln = row["Uln"];

        var approvalCreatedEvent = await _learningDataSeeder.CreateLearner(actualStartDate,  endDate, trainingPrice, epaPrice, plannedStartDate, uln);

        _scenarioContext.SetApprenticeshipCreatedEvent(approvalCreatedEvent);

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learnerKey = dbConnection.GetLearningKey(_scenarioContext.GetApprenticeshipCreatedEvent().Uln);
        _scenarioContext.SetLearningKey(learnerKey);
    }

    [Then(@"an Apprenticeship record is created")]
    public async Task ThenAnApprenticeshipRecordIsCreated()
    {
        await WaitHelper.WaitForIt(async () => await ApprenticeshipRecordMatchesExpectation(), "Failed to find the apprenticeship record");

        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);

        var learner = dbConnection.GetLearner(ApprovalCreatedEvent.Uln);
        var apprenticeship = dbConnection.GetLearningByLearnerKey(learner.Key);

        apprenticeship.Should().NotBeNull();
        learner.Uln.Should().Be(ApprovalCreatedEvent.Uln);
        learner.FirstName.Should().Be(ApprovalCreatedEvent.FirstName);
        learner.LastName.Should().Be(ApprovalCreatedEvent.LastName);
        apprenticeship.Key.Should().NotBe(Guid.Empty);
        apprenticeship.ApprovalsApprenticeshipId.Should().Be(ApprovalCreatedEvent.ApprenticeshipId);
       
        var episode = (await dbConnection.GetAllAsync<ApprenticeshipEpisode>()).Last(x => x.LearningKey == apprenticeship.Key);
        episode.Should().NotBeNull();
        episode.Key.Should().NotBe(Guid.Empty);
        episode.Ukprn.Should().Be(ApprovalCreatedEvent.ProviderId);
        episode.EmployerAccountId.Should().Be(ApprovalCreatedEvent.AccountId);
        episode.FundingEmployerAccountId.Should().Be(ApprovalCreatedEvent.TransferSenderId);
        episode.LegalEntityName.Should().Be(ApprovalCreatedEvent.LegalEntityName);
        episode.FundingPlatform.Should().Be(ApprovalCreatedEvent.IsOnFlexiPaymentPilot.HasValue ? (ApprovalCreatedEvent.IsOnFlexiPaymentPilot.Value ? FundingPlatform.DAS : FundingPlatform.SLD) : null);
        int.Parse(episode.TrainingCode).Should().Be(int.Parse(ApprovalCreatedEvent.TrainingCode));

        var episodePrice = (await dbConnection.GetAllAsync<EpisodePrice>()).Last(x => x.EpisodeKey == episode.Key);
        episodePrice.Should().NotBeNull();
        episodePrice.StartDate.Should().BeSameDateAs(ApprovalCreatedEvent.ActualStartDate ?? ApprovalCreatedEvent.StartDate);
        episodePrice.EndDate.Should().BeSameDateAs(ApprovalCreatedEvent.EndDate);
        episodePrice.TotalPrice.Should().Be(ApprovalCreatedEvent.PriceEpisodes[0].Cost);

        _scenarioContext["Learning"] = apprenticeship;
        _scenarioContext["Episode"] = episode;
        _scenarioContext["EpisodePrice"] = episodePrice;
    }

    [Then("an Apprenticeship record is not created")]
    public async Task ThenAnApprenticeshipRecordIsNotCreated()
    {
        await WaitHelper.WaitForUnexpected(ApprenticeshipRecordMatchesExpectation, "Found unexpected apprenticeship record created.");
    }

    private async Task<bool> ApprenticeshipRecordMatchesExpectation()
    {
        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);
        
        var apprenticeship = dbConnection.GetLearning(ApprovalCreatedEvent.Uln);
        
        return apprenticeship != null;
    }

    [Then(@"an ApprenticeshipCreatedEvent event is published")]
    public async Task ThenAnApprenticeshipCreatedEventEventIsPublished()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<LearningCreatedEvent>().Any(EventMatchesExpectation), $"Failed to find published {nameof(LearningCreatedEvent)} event");

        var publishedEvent = _testContext.MessageSession.ReceivedEvents<LearningCreatedEvent>().Single(EventMatchesExpectation);

        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);

        var learner = dbConnection.GetLearner(ApprovalCreatedEvent.Uln);
        var apprenticeship = dbConnection.GetLearningByLearnerKey(learner.Key);

        publishedEvent.Uln.Should().Be(learner.Uln);
        publishedEvent.LearningKey.Should().Be(Apprenticeship.Key);
        int.Parse(publishedEvent.Episode.TrainingCode).Should().Be(int.Parse(LatestEpisode.TrainingCode));
        publishedEvent.Episode.Prices.MaxBy(x => x.StartDate)?.StartDate.Should().BeSameDateAs(LatestEpisodePrice.StartDate);
        publishedEvent.Episode.Prices.MaxBy(x => x.StartDate)?.EndDate.Should().BeSameDateAs(LatestEpisodePrice.EndDate);
        publishedEvent.Episode.Prices.MaxBy(x => x.StartDate)?.TotalPrice.Should().Be(LatestEpisodePrice.TotalPrice);
        publishedEvent.ApprovalsApprenticeshipId.Should().Be(Apprenticeship.ApprovalsApprenticeshipId);
        publishedEvent.Episode.EmployerAccountId.Should().Be(LatestEpisode.EmployerAccountId);
        publishedEvent.Episode.FundingEmployerAccountId.Should().Be(LatestEpisode.FundingEmployerAccountId);
        publishedEvent.Episode.FundingType.ToString().Should().Be(LatestEpisode.FundingType.ToString());
        publishedEvent.Episode.LegalEntityName.Should().Be(LatestEpisode.LegalEntityName);
        publishedEvent.Episode.Ukprn.Should().Be(LatestEpisode.Ukprn);
        publishedEvent.FirstName.Should().Be(learner.FirstName);
        publishedEvent.LastName.Should().Be(learner.LastName);
        publishedEvent.Episode.FundingPlatform.ToString().Should().Be(LatestEpisode.FundingPlatform.ToString());

        _scenarioContext["publishedEvent"] = publishedEvent;
    }

    [Then(@"an ApprenticeshipCreatedEvent event is not published")]
    public async Task ThenAnApprenticeshipCreatedEventEventIsNotPublished()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<LearningCreatedEvent>().Any(EventMatchesExpectation), $"Found unexpected {nameof(LearningCreatedEvent)} event");
    }


    private bool EventMatchesExpectation(LearningCreatedEvent learningCreatedEvent)
    {
        return learningCreatedEvent.Uln == ApprovalCreatedEvent.Uln;
    }

    public CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent ApprovalCreatedEvent => _scenarioContext.GetApprenticeshipCreatedEvent();
    public DataAccess.Entities.Learning.ApprenticeshipLearning Apprenticeship => (DataAccess.Entities.Learning.ApprenticeshipLearning)_scenarioContext["Learning"];
    public ApprenticeshipEpisode LatestEpisode => (ApprenticeshipEpisode)_scenarioContext["Episode"];
    public EpisodePrice LatestEpisodePrice => (EpisodePrice)_scenarioContext["EpisodePrice"];
}
