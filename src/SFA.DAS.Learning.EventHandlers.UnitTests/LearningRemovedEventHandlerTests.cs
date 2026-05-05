using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearningRemovedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IMessageSession> _messageSession;
    private ILogger<LearningRemovedEventHandler> _logger;
    private LearningRemovedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _logger = Mock.Of<ILogger<LearningRemovedEventHandler>>();

        _handler = new LearningRemovedEventHandler(_messageSession.Object, _logger);
    }

    [Test]
    public async Task Handle_PublishesLearningRemovedEventWithCorrectFields()
    {
        // Arrange
        var domainEvent = _fixture.Create<Domain.Events.LearningRemovedEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _messageSession.Verify(x => x.Publish(
                It.Is<Types.LearningRemovedEvent>(msg =>
                    msg.LearningKey == domainEvent.LearningKey &&
                    msg.ApprenticeshipId == domainEvent.ApprenticeshipId
                ),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
