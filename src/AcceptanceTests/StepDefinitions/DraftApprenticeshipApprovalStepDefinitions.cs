using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.AcceptanceTests.StepDefinitions;

[Binding]
public class DraftApprenticeshipApprovalStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public DraftApprenticeshipApprovalStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When(@"the draft apprenticeship is approved by the approvals journey")]
    public async Task WhenTheDraftApprenticeshipIsApprovedByTheApprovalsJourney()
    {
        await _testContext.TestFunction!.PublishEvent(ApprovalCreatedEvent);
    }

    [Then(@"the existing apprenticeship record is approved and not duplicated")]
    public async Task ThenTheExistingApprenticeshipRecordIsApprovedAndNotDuplicated()
    {
        await WaitHelper.WaitForIt(async () => await IsApprenticeshipApproved(), "Failed to find the approved apprenticeship record");

        await using var dbConnection = new SqlConnection(_scenarioContext.GetDbConnectionString());
        var learner = dbConnection.GetLearner(ApprovalCreatedEvent.Uln);
        var apprenticeships = dbConnection.GetAll<ApprenticeshipLearning>().Where(x => x.LearnerKey == learner.Key).ToList();

        apprenticeships.Should().ContainSingle("approving an existing draft must not create a duplicate apprenticeship");

        var learning = dbConnection.GetLearningByLearnerKey(learner.Key);
        var episode = learning.Episodes.Single();

        episode.IsApproved.Should().BeTrue();
        episode.ApprovalsApprenticeshipId.Should().Be(ApprovalCreatedEvent.ApprenticeshipId);
        episode.EmployerAccountId.Should().Be(ApprovalCreatedEvent.AccountId);
        episode.FundingEmployerAccountId.Should().Be(ApprovalCreatedEvent.TransferSenderId);
        episode.LegalEntityName.Should().Be(ApprovalCreatedEvent.LegalEntityName);
        episode.AccountLegalEntityId.Should().Be(ApprovalCreatedEvent.AccountLegalEntityId);

        _scenarioContext["Learning"] = learning;
        _scenarioContext["Episode"] = episode;
    }

    [Then(@"a LearningApprovedEvent event is published")]
    public async Task ThenALearningApprovedEventEventIsPublished()
    {
        await WaitHelper.WaitForIt(
            () => _testContext.MessageSession.ReceivedEvents<LearningApprovedEvent>().Any(EventMatchesExpectation),
            $"Failed to find published {nameof(LearningApprovedEvent)} event");

        var publishedEvent = _testContext.MessageSession.ReceivedEvents<LearningApprovedEvent>().Single(EventMatchesExpectation);

        publishedEvent.ApprovalsApprenticeshipId.Should().Be(ApprovalCreatedEvent.ApprenticeshipId);
        publishedEvent.EmployerAccountId.Should().Be(ApprovalCreatedEvent.AccountId);
    }

    private async Task<bool> IsApprenticeshipApproved()
    {
        await using var dbConnection = new SqlConnection(_testContext.SqlDatabase?.DatabaseInfo.ConnectionString);

        var learner = dbConnection.GetAll<Learner>().SingleOrDefault(x => x.Uln == ApprovalCreatedEvent.Uln);
        if (learner == null) return false;

        var apprenticeships = dbConnection.GetAll<ApprenticeshipLearning>().Where(x => x.LearnerKey == learner.Key).ToList();
        if (apprenticeships.Count != 1) return false;

        var episode = dbConnection.GetAll<ApprenticeshipEpisode>().SingleOrDefault(x => x.LearningKey == apprenticeships.Single().Key);
        return episode is { IsApproved: true };
    }

    private bool EventMatchesExpectation(LearningApprovedEvent @event)
    {
        var learning = (DataAccess.Entities.Learning.ApprenticeshipLearning)_scenarioContext["Learning"];
        return @event.LearningKey == learning.Key;
    }

    public CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent ApprovalCreatedEvent => _scenarioContext.GetApprenticeshipCreatedEvent();
}
