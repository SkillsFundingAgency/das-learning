using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGettingShortCoursesByAcademicYear
{
    private LearningDataContext _dbContext;
    private GetShortCoursesByAcademicYearQueryHandler _sut;

    // Academic year 2425 = 2024-08-01 to 2025-07-31
    private const long UkPrn = 10005001;
    private const int AcademicYear = 2425;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetShortCoursesByAcademicYearQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenApprovedCoursesInDateRangeAreReturned()
    {
        // Arrange
        var (learning, learner) = await SeedShortCourse(isApproved: true, startDate: new DateTime(2024, 8, 1), expectedEndDate: new DateTime(2025, 7, 31));
        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        var item = result.Items.Single();
        item.Key.Should().Be(learning.Key);
        item.Uln.Should().Be(learner.Uln);
    }

    [Test]
    public async Task ThenUnapprovedCoursesAreExcluded()
    {
        // Arrange
        await SeedShortCourse(isApproved: false, startDate: new DateTime(2024, 8, 1), expectedEndDate: new DateTime(2025, 7, 31));
        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenCoursesEndingBeforeAcademicYearStartAreExcluded()
    {
        // Arrange — ends before 2024-08-01
        await SeedShortCourse(isApproved: true, startDate: new DateTime(2023, 9, 1), expectedEndDate: new DateTime(2024, 7, 31));
        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenCoursesStartingAfterAcademicYearEndAreExcluded()
    {
        // Arrange — starts after 2025-07-31
        await SeedShortCourse(isApproved: true, startDate: new DateTime(2025, 8, 1), expectedEndDate: new DateTime(2026, 6, 30));
        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenCoursesWithdrawnBeforeAcademicYearStartAreExcluded()
    {
        // Arrange — withdrawn before academic year begins
        await SeedShortCourse(isApproved: true, startDate: new DateTime(2024, 1, 1), expectedEndDate: new DateTime(2025, 1, 1), withdrawalDate: new DateTime(2024, 7, 31));
        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenPaginationIsApplied()
    {
        // Arrange — 3 matching courses, page size 2
        for (var i = 0; i < 3; i++)
            await SeedShortCourse(isApproved: true, startDate: new DateTime(2024, 8, 1), expectedEndDate: new DateTime(2025, 7, 31));

        var query = new GetShortCoursesByAcademicYearRequest(UkPrn, AcademicYear, 1, 2);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.Page.Should().Be(1);
    }

    private async Task<(ShortCourseLearning, Learner)> SeedShortCourse(
        bool isApproved,
        DateTime startDate,
        DateTime expectedEndDate,
        DateTime? withdrawalDate = null)
    {
        var learnerKey = Guid.NewGuid();
        var learner = new Learner { Key = learnerKey, Uln = Guid.NewGuid().ToString()[..10], FirstName = "A", LastName = "B" };
        _dbContext.LearnersDbSet.Add(learner);

        var learning = new ShortCourseLearning { Key = Guid.NewGuid() };
        learning.LearnerKey = learnerKey;
        learning.Episodes.Add(new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = UkPrn,
            TrainingCode = "SC-001",
            IsApproved = isApproved,
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            WithdrawalDate = withdrawalDate,
            Price = 1000m
        });
        _dbContext.ShortCourseLearnings.Add(learning);

        await _dbContext.SaveChangesAsync();
        return (learning, learner);
    }
}
