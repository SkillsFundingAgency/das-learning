using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
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
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningHistory>()), Times.Once);

        capturedHistory.Should().NotBeNull();

        using (new AssertionScope())
        {
            capturedHistory!.LearningKey.Should().Be(domainEvent.LearningKey);
            capturedHistory.AcademicYear.Should().Be(domainEvent.AcademicYear);
            capturedHistory.Operation.Should().Be(nameof(ApprenticeshipLearningOperation.Updated));
            capturedHistory.Changes.Should().Contain("PersonalDetails").And.Contain("Reinstated");
            capturedHistory.State.Should().Contain(domainEvent.Snapshot.LearningKey.ToString());
            capturedHistory.State.Should().NotContain(nameof(ApprenticeshipLearningChangedEvent.AcademicYear));
            capturedHistory.State.Should().NotContain(nameof(ApprenticeshipLearningChangedEvent.Operation));
        }
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
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        capturedHistory.Should().NotBeNull();
        capturedHistory!.Changes.Should().BeNull();
    }

    [Test]
    public async Task Handle_WhenFurtherApprovalIsCleared_RecordsTheOperationAndTheSnapshotFlag()
    {
        // Arrange
        var domainEvent = _fixture.Build<ApprenticeshipLearningChangedEvent>()
            .With(x => x.Operation, ApprenticeshipLearningOperation.FurtherApproved)
            .With(x => x.Changes, new List<LearningUpdateChanges>())
            .Create();
        domainEvent.Snapshot.Episodes.ForEach(e => e.FurtherApprovalNeeded = false);

        ApprenticeshipLearningHistory? capturedHistory = null;
        _repository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningHistory>()))
            .Callback<ApprenticeshipLearningHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        capturedHistory.Should().NotBeNull();

        using (new AssertionScope())
        {
            capturedHistory!.Operation.Should().Be("FurtherApproved");
            capturedHistory.State.Should().Contain("\"FurtherApprovalNeeded\": false");
        }
    }
}
