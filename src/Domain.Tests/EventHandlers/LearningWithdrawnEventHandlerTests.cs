using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.EventHandlers;
using SFA.DAS.Learning.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Domain.UnitTests.EventHandlers
{
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
                    It.Is<Types.LearningWithdrawnEvent>(msg =>
                        msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                        msg.LearningKey == domainEvent.LearningKey &&
                        msg.LastDayOfLearning == domainEvent.LastDayOfLearning &&
                        msg.EmployerAccountId == domainEvent.EmployerAccountId &&
                        msg.Reason == domainEvent.Reason
                        ),
                    It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}