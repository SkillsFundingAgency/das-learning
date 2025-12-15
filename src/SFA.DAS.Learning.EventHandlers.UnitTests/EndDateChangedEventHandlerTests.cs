using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests
{
    public class EndDateChangedEventHandlerTests
    {
        private IFixture _fixture;
        private Mock<IMessageSession> _messageSession;
        private ILogger<EndDateChangedEventHandler> _logger;
        private EndDateChangedEventHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _messageSession = new Mock<IMessageSession>();
            _logger = Mock.Of<ILogger<EndDateChangedEventHandler>>();

            _handler = new EndDateChangedEventHandler(_messageSession.Object, _logger);
        }

        [Test]
        public async Task Handle_PublishesEndDateChangedEventMessage()
        {
            // Arrange
            var domainEvent = _fixture.Create<EndDateChangedEvent>();

            // Act
            await _handler.Handle(domainEvent, default);

            _messageSession.Verify(x => x.Publish(
                    It.Is<Types.EndDateChangedEvent>(msg =>
                        msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                        msg.LearningKey == domainEvent.LearningKey &&
                        msg.PlannedEndDate == domainEvent.PlannedEndDate),
                    It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}