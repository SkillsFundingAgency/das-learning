using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.MessageHandlers.UnitTests;

public class ApprenticeshipLearningChangedEventHandlerTests
{
    private IFixture _fixture;
    private Mock<IApprenticeshipLearningHistoryRepository> _repository;
    private ILogger<ApprenticeshipLearningChangedEventHandler> _logger;
    private ApprenticeshipLearningChangedEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _repository = new Mock<IApprenticeshipLearningHistoryRepository>();
        _logger = Mock.Of<ILogger<ApprenticeshipLearningChangedEventHandler>>();

        _handler = new ApprenticeshipLearningChangedEventHandler(
            _repository.Object,
            _logger);
    }

    [Test]
    public async Task Handle_ArchivesApprenticeshipLearningHistory()
    {
        // Arrange
        var domainEvent = _fixture.Build<ApprenticeshipLearningChangedEvent>()
            .With(x => x.Operation, ApprenticeshipLearningOperation.Updated)
            .With(x => x.Changes, [LearningUpdateChanges.PersonalDetails, LearningUpdateChanges.Reinstated])
            .Create();

        ApprenticeshipLearningHistory? capturedHistory = null;
        _repository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningHistory>()))
            .Callback<ApprenticeshipLearningHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningHistory>()), Times.Once);

        Assert.That(capturedHistory, Is.Not.Null);
        Assert.That(capturedHistory!.LearningKey, Is.EqualTo(domainEvent.LearningKey));
        Assert.That(capturedHistory.AcademicYear, Is.EqualTo(domainEvent.AcademicYear));
        Assert.That(capturedHistory.Operation, Is.EqualTo(nameof(ApprenticeshipLearningOperation.Updated)));
        Assert.That(capturedHistory.Changes, Does.Contain("PersonalDetails").And.Contain("Reinstated"));

        // State is the aggregate Snapshot only — never the envelope fields (AcademicYear/Operation/Changes),
        // so a diff across two history rows shows the apprenticeship's own evolution, not request-context noise.
        Assert.That(capturedHistory.State, Does.Contain(domainEvent.Snapshot.LearningKey.ToString()));
        Assert.That(capturedHistory.State, Does.Not.Contain(nameof(ApprenticeshipLearningChangedEvent.AcademicYear)));
        Assert.That(capturedHistory.State, Does.Not.Contain(nameof(ApprenticeshipLearningChangedEvent.Operation)));
    }

    [Test]
    public async Task Handle_WhenNoChanges_ChangesFieldIsNull()
    {
        // Arrange
        var domainEvent = _fixture.Build<ApprenticeshipLearningChangedEvent>()
            .With(x => x.Operation, ApprenticeshipLearningOperation.Created)
            .With(x => x.Changes, new List<LearningUpdateChanges>())
            .Create();

        ApprenticeshipLearningHistory? capturedHistory = null;
        _repository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningHistory>()))
            .Callback<ApprenticeshipLearningHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, default);

        // Assert
        Assert.That(capturedHistory, Is.Not.Null);
        Assert.That(capturedHistory!.Changes, Is.Null);
    }
}
