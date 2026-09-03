using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenCheckingApprovedApprenticeshipExists
{
    private LearningDataContext _dbContext;
    private CheckApprovedApprenticeshipExistsQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new CheckApprovedApprenticeshipExistsQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    private async Task<Guid> AddLearnerAndLearning(string uln, string trainingCode = "123")
    {
        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = uln, FirstName = "Jane", LastName = "Doe" });

        var learningKey = Guid.NewGuid();
        var learning = new ApprenticeshipLearning { Key = learningKey, LearnerKey = learnerKey, TrainingCode = trainingCode };
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        return learningKey;
    }

    private async Task AddEpisode(Guid learningKey, long ukprn, bool isApproved, bool isRemoved, DateTime startDate)
    {
        var learning = await _dbContext.ApprenticeshipLearningDbSet
            .Include(al => al.Episodes)
            .ThenInclude(e => e.Prices)
            .SingleAsync(al => al.Key == learningKey);

        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = learningKey,
            Ukprn = ukprn,
            IsApproved = isApproved,
            IsRemoved = isRemoved,
            LegalEntityName = "Test",
            ApprovalsApprenticeshipId = 1
        };
        episode.Prices.Add(new EpisodePrice { Key = Guid.NewGuid(), EpisodeKey = episode.Key, StartDate = startDate, EndDate = startDate.AddYears(1), TotalPrice = 1000 });

        learning.Episodes.Add(episode);
        await _dbContext.SaveChangesAsync();
    }

    [Test]
    public async Task ThenReturnsTrueWhenFound()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenReturnsFalseWhenNoMatchingLearner()
    {
        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "9999999999", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeFalse();
    }

    [Test]
    public async Task ThenReturnsFalseWhenTrainingCodeDoesNotMatch()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "999", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeFalse();
    }

    [Test]
    public async Task ThenReturnsFalseWhenApprovalStateDoesNotMatchRequestedIsApproved()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: false, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeFalse();
    }

    [Test]
    public async Task ThenReturnsTrueForUnapprovedMatchWhenRequestedIsApprovedIsFalse()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: false, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 15), false);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenRemovedButApprovedEpisodeStillCountsAsExisting()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: true, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenMatchesOnStartDateMonthAndYearOnlyIgnoringDay()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 28), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenReturnsFalseWhenStartDateMonthYearDoesNotMatch()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 10, 1), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeFalse();
    }

    [Test]
    public async Task ThenMatchesEarlierEpisodeWhenLearnerHasMultipleEpisodesForSameProviderAndCourse()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2026, 1, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenMatchesLaterEpisodeWhenLearnerHasMultipleEpisodesForSameProviderAndCourse()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2026, 1, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(1000, "1111111111", "123", new DateTime(2026, 1, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task ThenReturnsFalseWhenUkprnDoesNotMatch()
    {
        var learningKey = await AddLearnerAndLearning("1111111111", "123");
        await AddEpisode(learningKey, 1000, isApproved: true, isRemoved: false, startDate: new DateTime(2025, 9, 1));

        var query = new CheckApprovedApprenticeshipExistsRequest(2000, "1111111111", "123", new DateTime(2025, 9, 15), true);

        var result = await _sut.Handle(query);

        result.Exists.Should().BeFalse();
    }
}
