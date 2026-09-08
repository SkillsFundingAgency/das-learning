using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class ShortCourseLearningChangedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IShortCourseLearningHistoryRepository> _repository;
    private ILogger<ShortCourseLearningChangedEventHandler> _logger;
    private ShortCourseLearningChangedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _repository = new Mock<IShortCourseLearningHistoryRepository>();
        _logger = Mock.Of<ILogger<ShortCourseLearningChangedEventHandler>>();

        _handler = new ShortCourseLearningChangedEventHandler(
            _repository.Object,
            _logger);
    }

    [Test]
    public async Task Handle_ArchivesShortCourseLearningHistory()
    {
        // Arrange
        var domainEvent = _fixture.Build<ShortCourseLearningChangedEvent>()
            .With(x => x.Operation, ShortCourseLearningOperation.Updated)
            .Create();

        ShortCourseLearningHistory? capturedHistory = null;
        _repository
            .Setup(x => x.Add(It.IsAny<ShortCourseLearningHistory>()))
            .Callback<ShortCourseLearningHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _repository.Verify(x => x.Add(It.IsAny<ShortCourseLearningHistory>()), Times.Once);

        Assert.That(capturedHistory, Is.Not.Null);
        Assert.That(capturedHistory!.LearningKey, Is.EqualTo(domainEvent.LearningKey));
        Assert.That(capturedHistory.AcademicYear, Is.EqualTo(domainEvent.AcademicYear));
        Assert.That(capturedHistory.Operation, Is.EqualTo(nameof(ShortCourseLearningOperation.Updated)));
        Assert.That(capturedHistory.State, Does.Contain(domainEvent.LearningKey.ToString()));
    }
}
