using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearningWithdrawnEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IMessageSession> _messageSession;
    private ILogger<LearningWithdrawnEventHandler> _logger;
    private LearningWithdrawnEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _logger = Mock.Of<ILogger<LearningWithdrawnEventHandler>>();

        _handler = new LearningWithdrawnEventHandler(_messageSession.Object, _logger);
    }

    [Test]
    public async Task Handle_PublishesLearningWithdrawnEventMessage()
    {
        // Arrange
        var domainEvent = _fixture.Build<LearningWithdrawnEvent>()
            .With(x => x.WithdrawnReasonCode, _fixture.Create<short>())
            .With(x => x.Created, _fixture.Create<DateTime>())
            .Create();

        // Act
        await _handler.Handle(domainEvent, default);

        _messageSession.Verify(x => x.Publish(
                It.Is<Types.LearningWithdrawnEvent>(msg =>
                    msg.ApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                    msg.LearningKey == domainEvent.LearningKey &&
                    msg.WithdrawalDate == domainEvent.LastDayOfLearning &&
                    msg.WithdrawnReasonCode == domainEvent.WithdrawnReasonCode &&
                    msg.Created == domainEvent.Created
                ),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}