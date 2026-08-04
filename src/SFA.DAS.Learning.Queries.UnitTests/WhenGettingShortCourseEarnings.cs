using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
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
            TrainingCode = "ABC123",
            Price = 1500m,
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "ABC123",
                    IsApproved = true,
                    StartDate = new DateTime(2024, 8, 1),
                    ExpectedEndDate = new DateTime(2025, 7, 31),
                    LearnerRef = "LRN123"
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
        item.LearnerKey.Should().Be(learnerKey);
        item.Learner.Uln.Should().Be(learner.Uln);
        item.Learner.FirstName.Should().Be(learner.FirstName);
        item.Learner.LastName.Should().Be(learner.LastName);
        item.Learner.DateOfBirth.Should().Be(learner.DateOfBirth);
        item.Episodes.Single().LearningKey.Should().Be(learning.Key);
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
            TrainingCode = "EP999",
            IsApproved = false,
            StartDate = new DateTime(2024, 8, 1),
            ExpectedEndDate = new DateTime(2025, 7, 31),
            LearnerRef = "LRN123",
            EmployerType = EmployerType.Levy
        };

        var learning = new ShortCourseLearning { Key = Guid.NewGuid(), TrainingCode = "XYZ999", Price = 750m, Episodes = [episode] };
        learning.LearnerKey = learnerKey;

        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        var resultEpisode = result.Items.Single().Episodes.Single();
        resultEpisode.CourseCode.Should().Be(learning.TrainingCode);
        resultEpisode.IsApproved.Should().Be(episode.IsApproved);
        resultEpisode.Price.Should().Be(learning.Price);
        resultEpisode.LearnerRef.Should().Be(episode.LearnerRef);
        resultEpisode.EmployerType.Should().Be(episode.EmployerType);
    }

    [Test]
    public async Task ThenCompletedLearningWithCompletionDateBeforeAcademicYearIsExcluded()
    {
        // Arrange
        const long ukPrn = 1000;
        const int collectionYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "111", FirstName = "A", LastName = "B" });

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "ABC001",
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "ABC001",
                    StartDate = new DateTime(2023, 8, 1),
                    ExpectedEndDate = new DateTime(2025, 7, 31),
                    CompletionDate = new DateTime(2024, 7, 31), // completed before this A/Y
                    LearnerRef = string.Empty
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

    [Test]
    public async Task ThenCompletedLearningWithCompletionDateWithinAcademicYearIsIncluded()
    {
        // Arrange
        const long ukPrn = 1000;
        const int collectionYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "222", FirstName = "A", LastName = "B" });

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "ABC002",
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "ABC002",
                    StartDate = new DateTime(2024, 8, 1),
                    ExpectedEndDate = new DateTime(2025, 7, 31),
                    CompletionDate = new DateTime(2025, 3, 15), // completed within this A/Y
                    LearnerRef = string.Empty
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
        result.TotalItems.Should().Be(1);
        result.Items.Single().Episodes.Single().LearningKey.Should().Be(learning.Key);
    }

    [Test]
    public async Task ThenLearningWithRemovedEpisodeIsExcluded()
    {
        const long ukPrn = 1000;
        const int collectionYear = 2425;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "333", FirstName = "A", LastName = "B" });

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "REM001",
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "REM001",
                    IsRemoved = true,
                    StartDate = new DateTime(2024, 8, 1),
                    ExpectedEndDate = new DateTime(2025, 7, 31),
                    LearnerRef = string.Empty
                }
            ]
        };
        learning.LearnerKey = learnerKey;
        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Handle(new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20));

        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenRemovedEpisodesAreExcludedFromLearning()
    {
        const long ukPrn = 1000;
        const int collectionYear = 2425;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "444", FirstName = "A", LastName = "B" });

        var activeEpisode = new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukPrn,
            TrainingCode = "ACT001",
            IsApproved = true,
            StartDate = new DateTime(2024, 8, 1),
            ExpectedEndDate = new DateTime(2025, 7, 31),
            LearnerRef = string.Empty
        };

        var removedEpisode = new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukPrn,
            TrainingCode = "REM001",
            IsApproved = true,
            IsRemoved = true,
            StartDate = new DateTime(2024, 8, 1),
            ExpectedEndDate = new DateTime(2025, 7, 31),
            LearnerRef = string.Empty
        };

        var learning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "ACT001",
            Episodes = [activeEpisode, removedEpisode]
        };
        learning.LearnerKey = learnerKey;
        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Handle(new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20));

        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.Single().Episodes.Should().HaveCount(1);
        result.Items.Single().Episodes.Should().NotContain(x => x.CourseCode == removedEpisode.TrainingCode);
    }

    [Test]
    public async Task ThenALearnerWithMultipleCoursesIsReturnedAsOneItem()
    {
        const long ukPrn = 1000;
        const int collectionYear = 2425;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "555", FirstName = "A", LastName = "B" });

        var learningOne = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "AAA111",
            Episodes = [new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "AAA111", StartDate = new DateTime(2024, 8, 1), ExpectedEndDate = new DateTime(2025, 7, 31), LearnerRef = "LRN1" }]
        };
        learningOne.LearnerKey = learnerKey;

        var learningTwo = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "BBB222",
            Episodes = [new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "BBB222", StartDate = new DateTime(2024, 8, 1), ExpectedEndDate = new DateTime(2025, 7, 31), LearnerRef = "LRN1" }]
        };
        learningTwo.LearnerKey = learnerKey;

        _dbContext.ShortCourseLearnings.AddRange(learningOne, learningTwo);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Handle(new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20));

        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);

        var item = result.Items.Single();
        item.LearnerKey.Should().Be(learnerKey);
        item.Episodes.Should().HaveCount(2);
        item.Episodes.Should().Contain(e => e.LearningKey == learningOne.Key && e.CourseCode == "AAA111");
        item.Episodes.Should().Contain(e => e.LearningKey == learningTwo.Key && e.CourseCode == "BBB222");
    }

    [Test]
    public async Task ThenPagingIsAppliedByLearner()
    {
        const long ukPrn = 1000;
        const int collectionYear = 2425;

        var splitLearnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = splitLearnerKey, Uln = "666", FirstName = "A", LastName = "B" });

        var splitLearningOne = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "SPLIT1",
            Episodes = [new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "SPLIT1", StartDate = new DateTime(2024, 8, 1), ExpectedEndDate = new DateTime(2025, 7, 31), LearnerRef = "LRN1" }],
            LearnerKey = splitLearnerKey
        };

        var splitLearningTwo = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "SPLIT2",
            Episodes = [new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "SPLIT2", StartDate = new DateTime(2024, 8, 1), ExpectedEndDate = new DateTime(2025, 7, 31), LearnerRef = "LRN1" }],
            LearnerKey = splitLearnerKey
        };

        var otherLearnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = otherLearnerKey, Uln = "777", FirstName = "C", LastName = "D" });

        var otherLearning = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            TrainingCode = "OTHER1",
            Episodes = [new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "OTHER1", StartDate = new DateTime(2024, 8, 1), ExpectedEndDate = new DateTime(2025, 7, 31), LearnerRef = "LRN2" }],
            LearnerKey = otherLearnerKey
        };

        _dbContext.ShortCourseLearnings.AddRange(splitLearningOne, splitLearningTwo, otherLearning);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.Handle(new GetShortCoursesForEarningsRequest(ukPrn, collectionYear, 1, 20));

        // Two distinct learners and 3 courses in total
        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);

        var splitLearnerItem = result.Items.Single(i => i.LearnerKey == splitLearnerKey);
        splitLearnerItem.Episodes.Should().HaveCount(2, "both of the split learner's courses must land on the same page, not be split across pages");
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
            TrainingCode = "OLD001",
            Episodes =
            [
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    Ukprn = ukPrn,
                    TrainingCode = "OLD001",
                    StartDate = new DateTime(2025, 8, 1), // starts after academic year ends
                    LearnerRef = "LRN123",
                    ExpectedEndDate = new DateTime(2026, 7, 31)
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
