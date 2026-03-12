using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGettingShortCourseEarnings
{
    private LearningDataContext _dbContext;
    private GetShortCoursesForEarningsQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetShortCoursesForEarningsQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenEarningsAreReturned()
    {
        // Arrange
        const long ukPrn = 1000;
        const int collectionYear = 2425;
        const int page = 1;
        const int pageSize = 20;

        var learnerKey = Guid.NewGuid();
        var learner = new Learner
        {
            Key = learnerKey,
            Uln = "1234567890",
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 6, 15)
        };

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "ABC123",
                    IsApproved = true,
                    Price = 1500m,
                    StartDate = new DateTime(2024, 8, 1),
                    ExpectedEndDate = new DateTime(2025, 7, 31)
                }
            ]
        };
        learning.LearnerKey = learnerKey;

        _dbContext.LearnersDbSet.Add(learner);
        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, page, pageSize);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(1);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Should().HaveCount(1);

        var item = result.Items.Single();
        item.LearningKey.Should().Be(learning.Key);
        item.Learner.Uln.Should().Be(learner.Uln);
        item.Learner.FirstName.Should().Be(learner.FirstName);
        item.Learner.LastName.Should().Be(learner.LastName);
        item.Learner.DateOfBirth.Should().Be(learner.DateOfBirth);
    }

    [Test]
    public async Task ThenEpisodeMappingIsCorrect()
    {
        // Arrange
        const long ukPrn = 1000;
        const int collectionYear = 2425;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "111", FirstName = "A", LastName = "B" });

        var episode = new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukPrn,
            TrainingCode = "XYZ999",
            IsApproved = false,
            Price = 750m,
            StartDate = new DateTime(2024, 8, 1),
            ExpectedEndDate = new DateTime(2025, 7, 31)
        };

        var learning = new ShortCourseLearning { Key = Guid.NewGuid(), Episodes = [episode] };
        learning.LearnerKey = learnerKey;

        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        var resultEpisode = result.Items.Single().Episodes.Single();
        resultEpisode.CourseCode.Should().Be(episode.TrainingCode);
        resultEpisode.IsApproved.Should().Be(episode.IsApproved);
        resultEpisode.Price.Should().Be(episode.Price);
    }

    [Test]
    public async Task ThenLearningsOutsideDateRangeAreExcluded()
    {
        // Arrange
        const long ukPrn = 1000;
        const int collectionYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "999", FirstName = "A", LastName = "B" });

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "OLD001",
                    StartDate = new DateTime(2023, 1, 1),
                    ExpectedEndDate = new DateTime(2023, 12, 31) // ends before academic year starts
                }
            ]
        };
        learning.LearnerKey = learnerKey;

        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
