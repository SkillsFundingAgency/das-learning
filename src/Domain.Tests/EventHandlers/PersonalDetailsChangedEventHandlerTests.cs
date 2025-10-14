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
    public class PersonalDetailsChangedEventHandlerTests
    {
        private IFixture _fixture;
        private Mock<IMessageSession> _messageSession;
        private ILogger<PersonalDetailsChangedEventHandler> _logger;
        private PersonalDetailsChangedEventHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _messageSession = new Mock<IMessageSession>();
            _logger = Mock.Of<ILogger<PersonalDetailsChangedEventHandler>>();

            _handler = new PersonalDetailsChangedEventHandler(_messageSession.Object, _logger);
        }

        [Test]
        public async Task Handle_PublishesPersonalDetailsChangedEvent()
        {
            // Arrange
            var domainEvent = _fixture.Create<PersonalDetailsChangedEvent>();

            // Act
            await _handler.Handle(domainEvent, default);

            _messageSession.Verify(x => x.Publish(
                    It.Is<Types.PersonalDetailsChangedEvent>(msg =>
                        msg.ApprovalsApprenticeshipId == domainEvent.ApprovalsApprenticeshipId &&
                        msg.LearningKey == domainEvent.LearningKey &&
                        msg.FirstName == domainEvent.FirstName &&
                        msg.LastName == domainEvent.LastName && 
                        msg.EmailAddress == domainEvent.EmailAddress),
                    It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}