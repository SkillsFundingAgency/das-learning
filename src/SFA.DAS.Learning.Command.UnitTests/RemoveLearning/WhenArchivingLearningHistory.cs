using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.ArchiveLearningHistory;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.UnitTests.RemoveLearning;

[TestFixture]
public class WhenArchivingLearningHistory
{
    private ArchiveLearningHistoryCommandHandler _commandHandler;
    private Mock<ILearningHistoryRepository> _learningHistoryRepository;
    private Mock<ILogger<ArchiveLearningHistoryCommandHandler>> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _learningHistoryRepository = new Mock<ILearningHistoryRepository>();
        _logger = new Mock<ILogger<ArchiveLearningHistoryCommandHandler>>();

        _commandHandler = new ArchiveLearningHistoryCommandHandler(
            _learningHistoryRepository.Object,
            _logger.Object);

        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenTheLearningHistoryIsArchived()
    {
        // Arrange
        var learnerUpdatedEvent = _fixture.Create<LearnerUpdatedEvent>();
        var command = new ArchiveLearningHistoryCommand(learnerUpdatedEvent);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningHistoryRepository.Verify(x =>
                x.Add(It.Is<LearningHistory>(h =>
                    h.LearningId == learnerUpdatedEvent.Key &&
                    h.State.Contains(learnerUpdatedEvent.Key.ToString()) &&
                    h.CreatedOn <= DateTime.UtcNow &&
                    h.Key != Guid.Empty)),
            Times.Once);
    }
}