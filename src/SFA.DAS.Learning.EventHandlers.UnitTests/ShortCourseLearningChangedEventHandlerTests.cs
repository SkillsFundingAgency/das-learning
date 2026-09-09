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
            .With(x => x.Changes, [ShortCourseUpdateChanges.WithdrawalDate, ShortCourseUpdateChanges.Milestone])
            .Create();

        ShortCourseLearningHistory? capturedHistory = null;
        _repository
            .Setup(x => x.Add(It.IsAny<ShortCourseLearningHistory>()))
            .Callback<ShortCourseLearningHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _repository.Verify(x => x.Add(It.IsAny<ShortCourseLearningHistory>()), Times.Once);

        Assert.That(capturedHistory, Is.Not.Null);
        Assert.That(capturedHistory!.LearningKey, Is.EqualTo(domainEvent.LearningKey));
        Assert.That(capturedHistory.AcademicYear, Is.EqualTo(domainEvent.AcademicYear));
        Assert.That(capturedHistory.State, Does.Contain(domainEvent.Snapshot.LearningKey.ToString()));
        Assert.That(capturedHistory.Operation, Is.EqualTo(nameof(ShortCourseLearningOperation.Updated)));
        Assert.That(capturedHistory.Changes, Does.Contain("WithdrawalDate").And.Contain("Milestone"));

        //AcademicYear and Operation are excluded from the json state (included in the db record as separate columns)
        Assert.That(capturedHistory.State, Does.Not.Contain(nameof(ShortCourseLearningChangedEvent.AcademicYear)));
        Assert.That(capturedHistory.State, Does.Not.Contain(nameof(ShortCourseLearningChangedEvent.Operation)));
    }
}
