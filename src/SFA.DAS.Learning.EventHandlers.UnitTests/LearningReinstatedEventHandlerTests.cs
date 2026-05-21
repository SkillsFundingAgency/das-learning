using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearningReinstatedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IMessageSession> _messageSession;
    private ILogger<LearningReinstatedEventHandler> _logger;
    private LearningReinstatedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _logger = Mock.Of<ILogger<LearningReinstatedEventHandler>>();

        _handler = new LearningReinstatedEventHandler(_messageSession.Object, _logger);
    }

    [Test]
    public async Task Handle_PublishesLearningReinstatedEventWithCorrectFields()
    {
        // Arrange
        var domainEvent = _fixture.Create<Domain.Events.LearningReinstatedEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _messageSession.Verify(x => x.Publish(
                It.Is<Types.LearningReinstatedEvent>(msg =>
                    msg.LearningKey == domainEvent.LearningKey &&
                    msg.ApprenticeshipId == domainEvent.ApprenticeshipId
                ),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
