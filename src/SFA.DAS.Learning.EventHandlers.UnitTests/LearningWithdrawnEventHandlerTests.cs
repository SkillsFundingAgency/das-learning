using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Enums;
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
        var domainEvent = _fixture.Create<LearningWithdrawnEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        _messageSession.Verify(x => x.Publish(
                It.Is<Types.ApprenticeshipWithdrawnEvent>(msg =>
                    msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                    msg.LearningKey == domainEvent.LearningKey &&
                    msg.LastDayOfLearning == domainEvent.LastDayOfLearning &&
                    msg.EmployerAccountId == domainEvent.EmployerAccountId &&
                    msg.Reason == WithdrawReason.WithdrawDuringLearning.ToString()
                ),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}