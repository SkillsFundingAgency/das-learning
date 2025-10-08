using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGetApprenticeshipsWithEpisodes
{
    private Fixture _fixture;
    private Mock<ILearningQueryRepository> _apprenticeshipQueryRepository;
    private GetLearningsWithEpisodesRequestQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _apprenticeshipQueryRepository = new Mock<ILearningQueryRepository>();
        _sut = new GetLearningsWithEpisodesRequestQueryHandler(_apprenticeshipQueryRepository.Object, Mock.Of<ILogger<GetLearningsWithEpisodesRequestQueryHandler>>());
    }

    [Test]
    public async Task ThenApprenticeshipsWithEpisodesAreReturned()
    {
        // Arrange
        var query = _fixture.Create<GetLearningsWithEpisodesRequest>();
        query.CollectionYear = 2021;
        query.CollectionPeriod = 1;
        var apprenticeships = _fixture.Create<List<LearningWithEpisodes>>();
        var expectedResponse = new GetLearningsWithEpisodesResponse{ Items = apprenticeships };

        _apprenticeshipQueryRepository
            .Setup(x => x.GetLearningsWithEpisodes(query.Ukprn, It.IsAny<DateTime>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<LearningWithEpisodes>{ Data = apprenticeships });

        // Act
        var actualResult = await _sut.Handle(query);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoApprenticeshipsWithEpisodesExist()
    {
        // Arrange
        var query = _fixture.Create<GetLearningsWithEpisodesRequest>();
        query.CollectionYear = 2021;
        query.CollectionPeriod = 1;

        _apprenticeshipQueryRepository
            .Setup(x => x.GetLearningsWithEpisodes(query.Ukprn, It.IsAny<DateTime>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<LearningWithEpisodes>());

        // Act
        var actualResult = await _sut.Handle(query);

        // Assert
        actualResult.Should().BeNull();
    }
}