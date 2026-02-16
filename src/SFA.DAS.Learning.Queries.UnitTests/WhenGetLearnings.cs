using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Queries.GetLearnings;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGetLearnings
{
    private Fixture _fixture;
    private Mock<ILearningQueryRepository> _apprenticeshipQueryRepository;
    private GetLearningsQueryHandler _sut;

    [SetUp]
    public void Setup()
    {

        _fixture = new Fixture();
        _apprenticeshipQueryRepository = new Mock<ILearningQueryRepository>();
        _sut = new GetLearningsQueryHandler(_apprenticeshipQueryRepository.Object);
    }

    [Test]
    public async Task ThenApprenticeshipsAreReturned()
    {
        //Arrange
        var query = _fixture.Create<GetLearningsRequest>();
        var expectedResult = _fixture.Create<IEnumerable<Models.Dtos.Learning>>();

        _apprenticeshipQueryRepository
            .Setup(x => x.GetAll(query.Ukprn, query.FundingPlatform))
            .ReturnsAsync(expectedResult);

        //Act
        var actualResult = await _sut.Handle(query);

        //Assert
        actualResult.Learnings.Should().BeEquivalentTo(expectedResult);
    }
}