using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearningDeletedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IMessageSession> _messageSession;
    private ILogger<LearningDeletedEventHandler> _logger;
    private LearningDeletedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _logger = Mock.Of<ILogger<LearningDeletedEventHandler>>();

        _handler = new LearningDeletedEventHandler(_messageSession.Object, _logger);
    }

    [Test]
    public async Task Handle_PublishesApprenticeshipWithdrawnEventWithCorrectFields()
    {
        // Arrange
        var domainEvent = _fixture.Create<LearningDeletedEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _messageSession.Verify(x => x.Publish(
                It.Is<Types.ApprenticeshipWithdrawnEvent>(msg =>
                    msg.LearningKey == domainEvent.LearningKey &&
                    msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                    msg.LastDayOfLearning == domainEvent.LastDayOfLearning &&
                    msg.EmployerAccountId == domainEvent.EmployerAccountId &&
                    msg.Reason == WithdrawReason.WithdrawFromStart.ToString()
                ),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
