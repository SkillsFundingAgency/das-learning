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
        var collectionYear = _fixture.Create<short>();
        var collectionPeriod = _fixture.Create<byte>();
        var expectedResponse = _fixture.Create<GetLearningsWithEpisodesResponse>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GetLearningsForFm36(ukprn, collectionYear, collectionPeriod);

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
        var collectionYear = _fixture.Create<short>();
        var collectionPeriod = _fixture.Create<byte>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn)))
            .ReturnsAsync((GetLearningsWithEpisodesResponse?)null);

        // Act
        var result = await _sut.GetLearningsForFm36(ukprn, collectionYear, collectionPeriod);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task ThenPagedApprenticeshipsAreReturnedWhenPaginationSpecified()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var collectionYear = _fixture.Create<short>();
        var collectionPeriod = _fixture.Create<byte>();
        var expectedResponse = _fixture.Create<GetLearningsWithEpisodesResponse>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(It.Is<GetLearningsWithEpisodesRequest>(r => r.Ukprn == ukprn)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GetLearningsForFm36(ukprn, collectionYear, collectionPeriod, 1, 2);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResponse);
    }
}