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
    public class WithdrawalRevertedEventHandlerTests
    {
        private IFixture _fixture;
        private Mock<IMessageSession> _messageSession;
        private ILogger<WithdrawalRevertedEventHandler> _logger;
        private WithdrawalRevertedEventHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _messageSession = new Mock<IMessageSession>();
            _logger = Mock.Of<ILogger<WithdrawalRevertedEventHandler>>();

            _handler = new WithdrawalRevertedEventHandler(_messageSession.Object, _logger);
        }

        [Test]
        public async Task Handle_PublishesWithdrawalRevertedEventMessage()
        {
            // Arrange
            var domainEvent = _fixture.Create<WithdrawalRevertedEvent>();

            // Act
            await _handler.Handle(domainEvent, default);

            _messageSession.Verify(x => x.Publish(
                    It.Is<Types.WithdrawalRevertedEvent>(msg =>
                        msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                        msg.LearningKey == domainEvent.LearningKey),
                    It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}