using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenIGetApprenticeshipsByAcademicYear
{
    private Fixture _fixture;
    private Mock<ILearningQueryRepository> _apprenticeshipQueryRepository;
    private Mock<ILearningOuterApiClient> _apiClient;
    private GetLearningsByAcademicYearQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _apprenticeshipQueryRepository = new Mock<ILearningQueryRepository>();
        _apiClient = new Mock<ILearningOuterApiClient>();
        _sut = new GetLearningsByAcademicYearQueryHandler(_apprenticeshipQueryRepository.Object);
    }

    [Test]
    public async Task ThenApprenticeshipsAreReturned()
    {
        //Arrange
        const int academicYear = 2526;
        const int pageSize = 20;
        const int pageNumber = 1;

        var queryResult = _fixture.Create<PagedResult<DataTransferObjects.Learning>>();
        var expectedResult = new GetLearningsByAcademicYearResponse
        {
            Items = queryResult.Data.Select(x => new GetLearningsByDatesResponseItem
            {
                Uln = x.Uln,
                Key = x.Key
            }),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = queryResult.TotalItems
        };

        var query = new GetLearningsByAcademicYearRequest(1000, academicYear, pageNumber, pageSize);

        var dates = AcademicYearParser.ParseFrom(academicYear);

        _apprenticeshipQueryRepository
            .Setup(x => x.GetByDates(query.UkPrn, dates, pageSize, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult)
            .Verifiable();

        //Act
        var actualResult = await _sut.Handle(query);

        //Assert
        actualResult.Should().BeEquivalentTo(expectedResult, x => x.ExcludingMissingMembers());

        _apprenticeshipQueryRepository.Verify();
        _apiClient.Verify();
    }
}