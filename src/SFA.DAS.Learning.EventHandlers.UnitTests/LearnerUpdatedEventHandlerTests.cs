using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ArchiveLearningHistory;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class LearnerUpdatedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<ICommandHandler<ArchiveLearningHistoryCommand>> _archiveCommandHandler;
    private ILogger<LearnerUpdatedEventHandler> _logger;
    private LearnerUpdatedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _archiveCommandHandler = new Mock<ICommandHandler<ArchiveLearningHistoryCommand>>();
        _logger = Mock.Of<ILogger<LearnerUpdatedEventHandler>>();

        _handler = new LearnerUpdatedEventHandler(
            _archiveCommandHandler.Object,
            _logger);
    }

    [Test]
    public async Task Handle_ArchivesLearningHistory()
    {
        // Arrange
        var domainEvent = _fixture.Create<LearnerUpdatedEvent>();

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _archiveCommandHandler.Verify(x =>
                x.Handle(
                    It.Is<ArchiveLearningHistoryCommand>(cmd =>
                        cmd.LearnerUpdatedEvent == domainEvent),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}