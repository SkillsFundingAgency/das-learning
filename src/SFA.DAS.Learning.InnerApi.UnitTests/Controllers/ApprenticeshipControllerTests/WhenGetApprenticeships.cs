using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ApprenticeshipControllerTests;

public class WhenGetApprenticeships
{
    private Fixture _fixture;
    private Mock<IQueryDispatcher> _queryDispatcher;
    private Mock<ILogger<LearningController>> _mockLogger;
    private LearningController _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _queryDispatcher = new Mock<IQueryDispatcher>();
        _mockLogger = new Mock<ILogger<LearningController>>();
        _sut = new LearningController(_queryDispatcher.Object, Mock.Of<ICommandDispatcher>(), Mock.Of<ILogger<LearningController>>(), Mock.Of<IPagedLinkHeaderService>());
    }

    [Test]
    public async Task ThenApprenticeshipsAreReturned()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learningKeys = _fixture.CreateMany<Guid>().ToList();
        var expectedResponse = _fixture.Create<GetLearningsWithEpisodesResponse>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GetLearningsForFm36(ukprn, learningKeys);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResponse.Items);
    }

    [Test]
    public async Task ThenNotFoundIsReturnedWhenNoRecordExists()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learningKeys = _fixture.CreateMany<Guid>().ToList();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn)))
            .ReturnsAsync((GetLearningsWithEpisodesResponse?)null);

        // Act
        var result = await _sut.GetLearningsForFm36(ukprn, learningKeys);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task ThenTheRequestedLearningKeysArePassedToTheQuery()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var learningKeys = _fixture.CreateMany<Guid>().ToList();
        var expectedResponse = _fixture.Create<GetLearningsWithEpisodesResponse>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.IsAny<GetLearningsWithEpisodesRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _sut.GetLearningsForFm36(ukprn, learningKeys);

        // Assert
        _queryDispatcher.Verify(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(
            It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn && r.LearningKeys.SequenceEqual(learningKeys))), Times.Once);
    }
}
