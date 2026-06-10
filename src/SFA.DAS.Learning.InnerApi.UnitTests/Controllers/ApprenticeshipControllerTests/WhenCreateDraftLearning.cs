using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ApprenticeshipControllerTests;

public class WhenCreateDraftLearning
{
    private Fixture _fixture;
    private Mock<IQueryDispatcher> _mockQueryDispatcher;
    private Mock<ICommandDispatcher> _mockCommandDispatcher;
    private Mock<ILogger<LearningController>> _mockLogger;
    private Mock<IPagedLinkHeaderService> _mockPagedLinkHeaderService;
    private LearningController _sut;

    [SetUp]
    public void Arrange()
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
            _mockPagedLinkHeaderService.Object
        );
    }

    [Test]
    public async Task ThenReturnsObjectResult()
    {
        // Arrange
        var request = _fixture.Create<CreateDraftApprenticeship>();
        var ukprn = _fixture.Create<long>();
        var uln = _fixture.Create<string>();
        var commandResult = _fixture.Create<CreateDraftApprenticeshipLearningCommandResult>();

        _mockCommandDispatcher
            .Setup(x => x.Send<CreateDraftApprenticeshipLearningCommand, CreateDraftApprenticeshipLearningCommandResult?>(
                It.IsAny<CreateDraftApprenticeshipLearningCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);

        // Act
        var result = await _sut.CreateDraftLearning(ukprn, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

    }
}
