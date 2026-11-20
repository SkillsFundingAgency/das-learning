using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class EmployerApprovalAppliedEventHandlerTests
{
    private Mock<IMessageSession> _messageSession;
    private EmployerApprovalAppliedEventHandler _handler;
    private Domain.Events.EmployerApprovalAppliedEvent _domainEvent;

    [SetUp]
    public void SetUp()
    {
        _messageSession = new Mock<IMessageSession>();
        _handler = new EmployerApprovalAppliedEventHandler(_messageSession.Object, Mock.Of<ILogger<EmployerApprovalAppliedEventHandler>>());

        _domainEvent = new Domain.Events.EmployerApprovalAppliedEvent
        {
            LearnerKey = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            EpisodeKey = Guid.NewGuid(),
            LearnerRef = "LEARNER-REF",
            ApprenticeshipId = 12345,
            LearningType = LearningType.FoundationApprenticeship,
            Changes = [new ApprovedChange("StartDate", "2025-07-01", "2025-08-01")]
        };
    }

    [Test]
    public async Task ThenApprovedLearningUpdatedEventIsPublishedEchoingTheChanges()
    {
        Types.ApprovedLearningUpdatedEvent? published = null;
        _messageSession
            .Setup(x => x.Publish(It.IsAny<Types.ApprovedLearningUpdatedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()))
            .Callback<object, PublishOptions, CancellationToken>((message, _, _) => published = (Types.ApprovedLearningUpdatedEvent)message);

        await _handler.Handle(_domainEvent, default);

        published.Should().NotBeNull();
        published!.LearningKey.Should().Be(_domainEvent.LearningKey);
        published.ApprenticeshipId.Should().Be(_domainEvent.ApprenticeshipId);
        published.LearningType.Should().Be("FoundationApprenticeship");
        published.Changes.Should().ContainSingle();
        published.Changes[0].ChangeType.Should().Be("StartDate");
        published.Changes[0].Data.Old.Should().Be("2025-07-01");
        published.Changes[0].Data.New.Should().Be("2025-08-01");
    }

    [Test]
    public async Task ThenApprovedLearningUpdatedEventIsPublishedEchoingTheApprovedPrices()
    {
        _domainEvent.Prices =
        [
            new ApprovedPrice(1000, 200, new DateTime(2025, 8, 1)),
            new ApprovedPrice(1500, null, new DateTime(2026, 2, 1))
        ];

        var published = await HandleAndCaptureApprovedLearningUpdatedEvent();

        published!.Prices.Should().BeEquivalentTo(new[]
        {
            new Types.PriceEpisode { TrainingPrice = 1000, AssessmentPrice = 200, EffectiveFrom = new DateTime(2025, 8, 1) },
            new Types.PriceEpisode { TrainingPrice = 1500, AssessmentPrice = 0, EffectiveFrom = new DateTime(2026, 2, 1) }
        }, o => o.WithStrictOrdering());
    }

    [Test]
    public async Task AndNoPricesWereApprovedThenTheEchoHasNoPrices()
    {
        var published = await HandleAndCaptureApprovedLearningUpdatedEvent();

        published!.Prices.Should().BeEmpty();
    }

    [Test]
    public async Task ThenEpisodeApprovedEventIsPublished()
    {
        await _handler.Handle(_domainEvent, default);

        _messageSession.Verify(x => x.Publish(
                It.Is<Types.EpisodeApprovedEvent>(msg =>
                    msg.LearnerKey == _domainEvent.LearnerKey &&
                    msg.LearningKey == _domainEvent.LearningKey &&
                    msg.EpisodeKey == _domainEvent.EpisodeKey &&
                    msg.LearnerRef == _domainEvent.LearnerRef),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task<Types.ApprovedLearningUpdatedEvent?> HandleAndCaptureApprovedLearningUpdatedEvent()
    {
        Types.ApprovedLearningUpdatedEvent? published = null;
        _messageSession
            .Setup(x => x.Publish(It.IsAny<Types.ApprovedLearningUpdatedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()))
            .Callback<object, PublishOptions, CancellationToken>((message, _, _) => published = (Types.ApprovedLearningUpdatedEvent)message);

        await _handler.Handle(_domainEvent, default);

        return published;
    }
}
