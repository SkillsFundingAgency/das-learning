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
    private readonly Mock<ILogger<ApprenticeshipController>> _mockLogger;
    private readonly Mock<IPagedLinkHeaderService> _mockPagedLinkHeaderService;
    private ApprenticeshipController _sut;

    public WhenRemoveLearning()
    {
        _fixture = new Fixture();
        _mockQueryDispatcher = new Mock<IQueryDispatcher>();
        _mockCommandDispatcher = new Mock<ICommandDispatcher>();
        _mockLogger = new Mock<ILogger<ApprenticeshipController>>();
        _mockPagedLinkHeaderService = new Mock<IPagedLinkHeaderService>();

        _sut = new ApprenticeshipController(
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
        var removedLearningKeys = _fixture.CreateMany<Guid>(2).ToList();

        _mockCommandDispatcher
            .Setup(x => x.Send<RemoveLearnerCommand, List<Guid>>(It.IsAny<RemoveLearnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(removedLearningKeys);

        // Act
        var result = await _sut.RemoveLearning(ukprn, learnerKey);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(removedLearningKeys);
    }

    [Test]
    public async Task ThenDispatchesRemoveLearnerCommandWithLearnerKey()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();

        _mockCommandDispatcher
            .Setup(x => x.Send<RemoveLearnerCommand, List<Guid>>(It.IsAny<RemoveLearnerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.RemoveLearning(ukprn, learnerKey);

        // Assert
        _mockCommandDispatcher.Verify(x => x.Send<RemoveLearnerCommand, List<Guid>>(
            It.Is<RemoveLearnerCommand>(c => c.LearnerKey == learnerKey),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}