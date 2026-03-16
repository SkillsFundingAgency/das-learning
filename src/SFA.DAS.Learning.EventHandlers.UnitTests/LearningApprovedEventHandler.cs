using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearningApprovedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IMessageSession> _messageSession;
    private ILogger<LearningApprovedEventHandler> _logger;
    private LearningApprovedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _logger = Mock.Of<ILogger<LearningApprovedEventHandler>>();

        _handler = new LearningApprovedEventHandler(_messageSession.Object, _logger);
    }

    [Test]
    public async Task Handle_PublishesLearningApprovedEvent()
    {
        // Arrange
        var domainEvent = _fixture.Create<LearningApprovedEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        _messageSession.Verify(x => x.Publish(
                It.Is<Types.LearningApprovedEvent>(e =>
                    e.LearningKey == domainEvent.LearningKey &&
                    e.EpisodeKey == domainEvent.EpisodeKey),
                It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}