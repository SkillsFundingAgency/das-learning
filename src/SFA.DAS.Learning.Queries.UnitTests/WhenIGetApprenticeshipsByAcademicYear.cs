using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenIGetApprenticeshipsByAcademicYear
{
    private LearningDataContext _dbContext;
    private GetLearningsByAcademicYearQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetLearningsByAcademicYearQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenApprenticeshipsAreReturned()
    {
        // Arrange
        const long ukPrn = 1000;
        const int academicYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "1234567890", FirstName = "A", LastName = "B" });

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), TrainingCode = "TC" };
        learning.LearnerKey = learnerKey;
        var episode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", LegalEntityName = "Test", ApprovalsApprenticeshipId = 1, IsApproved = true };
        episode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), TotalPrice = 5000 });
        learning.Episodes.Add(episode);
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsByAcademicYearRequest(ukPrn, academicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items.Should().HaveCount(1);
        result.Items.Single().Uln.Should().Be("1234567890");
        result.Items.Single().Key.Should().Be(learnerKey);
    }

    [Test]
    public async Task ThenALearnerWithMultipleApprenticeshipsIsReturnedOnlyOnce()
    {
        // Arrange
        const long ukPrn = 1000;
        const int academicYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "1234567890", FirstName = "A", LastName = "B" });

        var firstLearning = new ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = learnerKey, TrainingCode = "TC" };
        var firstEpisode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", LegalEntityName = "Test", ApprovalsApprenticeshipId = 1, IsApproved = true };
        firstEpisode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), TotalPrice = 5000 });
        firstLearning.Episodes.Add(firstEpisode);
        _dbContext.ApprenticeshipLearningDbSet.Add(firstLearning);

        var secondLearning = new ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = learnerKey, TrainingCode = "TC" };
        var secondEpisode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "456", LegalEntityName = "Test", ApprovalsApprenticeshipId = 2, IsApproved = true };
        secondEpisode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), TotalPrice = 5000 });
        secondLearning.Episodes.Add(secondEpisode);
        _dbContext.ApprenticeshipLearningDbSet.Add(secondLearning);

        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsByAcademicYearRequest(ukPrn, academicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.Single().Uln.Should().Be("1234567890");
        result.Items.Single().Key.Should().Be(learnerKey);
    }

    [Test]
    public async Task ThenDraftApprenticeshipsAreNotReturned()
    {
        // Arrange
        const long ukPrn = 1000;
        const int academicYear = 2425; // 2024-08-01 to 2025-07-31

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "1234567890", FirstName = "A", LastName = "B" });

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), TrainingCode = "TC" };
        learning.LearnerKey = learnerKey;
        var episode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", LegalEntityName = "Test", ApprovalsApprenticeshipId = 0, IsApproved = false };
        episode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), TotalPrice = 5000 });
        learning.Episodes.Add(episode);
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsByAcademicYearRequest(ukPrn, academicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task ThenApprenticeshipsAreNotReturnedWhenAllEpisodesAreRemoved()
    {
        // Arrange
        const long ukPrn = 1000;
        const int academicYear = 2425;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "1234567890", FirstName = "A", LastName = "B" });

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), TrainingCode = "TC" };
        learning.LearnerKey = learnerKey;
        var episode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", LegalEntityName = "Test", ApprovalsApprenticeshipId = 1, IsRemoved = true, IsApproved = true };
        episode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), TotalPrice = 5000 });
        learning.Episodes.Add(episode);
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsByAcademicYearRequest(ukPrn, academicYear, 1, 20);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.TotalItems.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

}
