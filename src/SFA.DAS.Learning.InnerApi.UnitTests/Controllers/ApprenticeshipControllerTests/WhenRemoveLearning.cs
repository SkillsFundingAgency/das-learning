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
    public async Task ThenReturns204()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();

        _mockCommandDispatcher
            .Setup(x => x.Send(It.IsAny<RemoveLearnerCommand>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _sut.RemoveLearning(ukprn, learnerKey);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}