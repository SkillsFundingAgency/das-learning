using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGettingShortCourseEarnings
{
    private Fixture _fixture;
    private Mock<IShortCourseLearningRepository> _mockRepository;
    private GetShortCoursesForEarningsQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<IShortCourseLearningRepository>();
        _sut = new GetShortCoursesForEarningsQueryHandler(_mockRepository.Object);
    }

    [Test]
    public async Task ThenEarningsAreReturned()
    {
        // Arrange
        const int collectionYear = 2425;
        const int page = 1;
        const int pageSize = 20;

        var queryResult = _fixture.Create<PagedResult<Models.Dtos.ShortCourseForEarnings>>();
        var query = new GetShortCoursesForEarningsRequest(1000, collectionYear, page, pageSize);
        var dates = AcademicYearParser.ParseFrom(collectionYear);

        _mockRepository
            .Setup(x => x.GetForEarnings(query.UkPrn, dates, pageSize, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult)
            .Verifiable();

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(queryResult.TotalItems);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Should().HaveSameCount(queryResult.Data);

        var firstExpected = queryResult.Data.First();
        var firstActual = result.Items.First();
        firstActual.LearningKey.Should().Be(firstExpected.LearningKey);
        firstActual.Learner.Uln.Should().Be(firstExpected.Uln);
        firstActual.Learner.FirstName.Should().Be(firstExpected.FirstName);
        firstActual.Learner.LastName.Should().Be(firstExpected.LastName);
        firstActual.Learner.DateOfBirth.Should().Be(firstExpected.DateOfBirth);
        firstActual.Episodes.Should().HaveSameCount(firstExpected.Episodes);

        _mockRepository.Verify();
    }

    [Test]
    public async Task ThenEpisodeMappingIsCorrect()
    {
        // Arrange
        const int collectionYear = 2425;
        var earning = _fixture.Create<Models.Dtos.ShortCourseForEarnings>();
        var queryResult = new PagedResult<Models.Dtos.ShortCourseForEarnings>
        {
            Data = new List<Models.Dtos.ShortCourseForEarnings> { earning },
            TotalItems = 1,
            TotalPages = 1
        };

        var query = new GetShortCoursesForEarningsRequest(1000, collectionYear, 1, 20);
        var dates = AcademicYearParser.ParseFrom(collectionYear);

        _mockRepository
            .Setup(x => x.GetForEarnings(query.UkPrn, dates, 20, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        var episode = result.Items.First().Episodes.First();
        var expectedEpisode = earning.Episodes.First();
        episode.CourseCode.Should().Be(expectedEpisode.CourseCode);
        episode.IsApproved.Should().Be(expectedEpisode.IsApproved);
    }
}
