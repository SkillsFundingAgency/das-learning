using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ApprenticeshipControllerTests;

public class WhenRemoveLearning
{
    private readonly Fixture _fixture;
    private readonly Mock<IQueryDispatcher> _mockQueryDispatcher;
    private readonly Mock<ICommandDispatcher> _mockCommandDispatcher;
    private readonly Mock<ILogger<LearningController>> _mockLogger;
    private readonly Mock<IPagedLinkHeaderService> _mockPagedLinkHeaderService;
    private LearningController _sut;

    public WhenRemoveLearning()
    {
        _fixture = new Fixture();
        _mockQueryDispatcher = new Mock<IQueryDispatcher>();
        _mockCommandDispatcher = new Mock<ICommandDispatcher>();
        _mockLogger = new Mock<ILogger<LearningController>>();
        _mockPagedLinkHeaderService = new Mock<IPagedLinkHeaderService>();

        _sut = new LearningController(
            _mockQueryDispatcher.Object,
            _mockCommandDispatcher.Object,
            _mockLogger.Object,
            _mockPagedLinkHeaderService.Object);
    }

    [Test]
    public async Task ThenReturnsRemovedLearningKeys()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();
        var academicYear = _fixture.Create<int>();
        var removedLearningKeys = _fixture.CreateMany<Guid>(2).ToList();

        _mockCommandDispatcher
            .Setup(x => x.Send<RemoveLearnerCommand, List<Guid>>(It.IsAny<RemoveLearnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(removedLearningKeys);

        // Act
        var result = await _sut.RemoveLearning(ukprn, learnerKey, academicYear);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(removedLearningKeys);
    }

    [Test]
    public async Task ThenDispatchesRemoveLearnerCommandWithLearnerKeyUkprnAndAcademicYear()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();
        var academicYear = _fixture.Create<int>();

        _mockCommandDispatcher
            .Setup(x => x.Send<RemoveLearnerCommand, List<Guid>>(It.IsAny<RemoveLearnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.RemoveLearning(ukprn, learnerKey, academicYear);

        // Assert
        _mockCommandDispatcher.Verify(x => x.Send<RemoveLearnerCommand, List<Guid>>(
            It.Is<RemoveLearnerCommand>(c => c.LearnerKey == learnerKey && c.Ukprn == ukprn && c.AcademicYear == academicYear),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}