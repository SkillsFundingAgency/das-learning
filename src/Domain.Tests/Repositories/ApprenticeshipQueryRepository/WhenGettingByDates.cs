using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository;

public class WhenGettingByDates
{
    private Domain.Repositories.LearningQueryRepository _sut;
    private Fixture _fixture;
    private LearningDataContext _dbContext;

    [SetUp]
    public void Arrange() => _fixture = new Fixture();

    [TearDown]
    public void CleanUp() => _dbContext.Dispose();

    [Test]
    public async Task ThenEmptyResponseIsReturnedWhenNoData()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var academicYear = new DateRange(new DateTime(2025, 8, 1), new DateTime(2026, 7, 31));
        SetUpApprenticeshipQueryRepository();

        var nonUkPrnApprenticeship = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, 10000, startDate: academicYear.Start.AddDays(4), learnerStatus: LearnerStatus.Active);

        var result = await _sut.GetByDates(ukprn, academicYear, 100, 0, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Count().Should().Be(0);
        result.Data.Select(x => x.Uln).Should().NotContain(nonUkPrnApprenticeship.Uln);
    }

    [Test]
    public async Task ThenCorrectApprenticeshipsForUkprnAreRetrievedForAcademicYearAndActive()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var academicYear = new DateRange(new DateTime(2025, 8, 1), new DateTime(2026, 7, 31));
        SetUpApprenticeshipQueryRepository();

        var apprenticeship1 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(-1), endDate: academicYear.End.AddDays(1), learnerStatus: LearnerStatus.Active);
        var apprenticeship2 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(-2), endDate: academicYear.End.AddDays(1), learnerStatus: LearnerStatus.Active);
        var apprenticeship3 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(-3), endDate: academicYear.End.AddDays(1), learnerStatus: LearnerStatus.Active);
        var apprenticeship4 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(-4), endDate: academicYear.End.AddDays(1), learnerStatus: LearnerStatus.Active);
        var nonUkPrnApprenticeship = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, 10000, startDate: academicYear.Start.AddDays(4), endDate: academicYear.End.AddDays(1), learnerStatus: LearnerStatus.Active);

        var result = await _sut.GetByDates(ukprn, academicYear, 100, 0, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Count().Should().Be(4);
        result.Data.Select(x => x.Uln).Should().Contain(apprenticeship1.Uln);
        result.Data.Select(x => x.Uln).Should().Contain(apprenticeship2.Uln);
        result.Data.Select(x => x.Uln).Should().Contain(apprenticeship3.Uln);
        result.Data.Select(x => x.Uln).Should().Contain(apprenticeship4.Uln);
        result.Data.Select(x => x.Uln).Should().NotContain(nonUkPrnApprenticeship.Uln);
    }

    [Test]
    public async Task ThenCorrectApprenticeshipsForUkprnAreRetrievedForAcademicYearAndActiveWithPagination()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var academicYear = new DateRange(new DateTime(2025, 8, 1), new DateTime(2026, 7, 31));
        SetUpApprenticeshipQueryRepository();

        const int totalItems = 20;
        for (var index = 0; index < totalItems; index++)
        {
            await _dbContext.AddApprenticeship(
                _fixture.Create<Guid>(),
                false,
                ukprn,
                startDate: academicYear.Start.AddDays(-1),
                endDate: academicYear.End.AddDays(1),
                learnerStatus: LearnerStatus.Active
            );
        }

        const int pageSize = 10;
        var result = await _sut.GetByDates(ukprn, academicYear, pageSize, 0, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Count().Should().Be(pageSize);
        result.TotalItems.Should().Be(totalItems);
        result.TotalPages.Should().Be((int)Math.Ceiling((double)totalItems / pageSize));
    }

    [Test]
    public async Task ThenCorrectApprenticeshipsForUkprnAreRetrievedForAcademicYearAndNotActive()
    {
        // Arrange
        var ukprn = _fixture.Create<long>();
        var academicYear = new DateRange(new DateTime(2025, 8, 1), new DateTime(2026, 7, 31));
        SetUpApprenticeshipQueryRepository();

        var apprenticeship1 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(1), learnerStatus: LearnerStatus.Withdrawn);
        var apprenticeship2 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(2), learnerStatus: LearnerStatus.Withdrawn);
        var apprenticeship3 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(3), learnerStatus: LearnerStatus.Withdrawn);
        var apprenticeship4 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, ukprn, startDate: academicYear.Start.AddDays(4), learnerStatus: LearnerStatus.Withdrawn);

        var result = await _sut.GetByDates(ukprn, academicYear, 100, 0, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Count().Should().Be(0);
        result.Data.Select(x => x.Uln).Should().NotContain(apprenticeship1.Uln);
        result.Data.Select(x => x.Uln).Should().NotContain(apprenticeship2.Uln);
        result.Data.Select(x => x.Uln).Should().NotContain(apprenticeship3.Uln);
        result.Data.Select(x => x.Uln).Should().NotContain(apprenticeship4.Uln);
    }

    private void SetUpApprenticeshipQueryRepository()
    {
        _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
        _sut = new LearningQueryRepository(
            new Lazy<LearningDataContext>(_dbContext),
            Mock.Of<ILogger<LearningQueryRepository>>()
        );
    }
}