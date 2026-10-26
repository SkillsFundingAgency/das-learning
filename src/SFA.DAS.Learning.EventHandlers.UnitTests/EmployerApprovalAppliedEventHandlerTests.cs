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
    public async Task ThenEpisodeApprovedEventIsPublished()
    {
        await _handler.Handle(_domainEvent, default);

        _messageSession.Verify(x => x.Publish(
                It.Is<Types.EpisodeApprovedEvent>(msg =>
                    msg.LearnerKey == _domainEvent.LearnerKey &&
                    msg.LearningKey == _domainEvent.LearningKey &&
                    msg.EpisodeKey == _domainEvent.EpisodeKey),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
