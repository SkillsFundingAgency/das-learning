using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGetApprenticeshipsWithEpisodes
{
    private const long Ukprn = 1000;

    private LearningDataContext _dbContext;
    private GetLearningsWithEpisodesRequestQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetLearningsWithEpisodesRequestQueryHandler(_dbContext, Mock.Of<ILogger<GetLearningsWithEpisodesRequestQueryHandler>>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenApprenticeshipsWithEpisodesAreReturned()
    {
        // Arrange
        var learning = await AddLearning("1111111111", approvalsId: 1);

        // Act
        var result = await _sut.Handle(QueryFor(learning));

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.Single().Key.Should().Be(learning.Key);
        result.Items.Single().Uln.Should().Be("1111111111");
    }

    [Test]
    public async Task ThenOnlyApprenticeshipsForTheRequestedKeysAreReturned()
    {
        // Arrange
        var requested1 = await AddLearning("1111111111", approvalsId: 1);
        var requested2 = await AddLearning("1111111112", approvalsId: 2);
        await AddLearning("1111111113", approvalsId: 3);

        // Act
        var result = await _sut.Handle(QueryFor(requested1, requested2));

        // Assert
        result.Should().NotBeNull();
        result!.Items.Select(x => x.Key).Should().BeEquivalentTo(new[] { requested1.Key, requested2.Key });
    }

    [Test]
    public async Task ThenApprenticeshipsForADifferentProviderAreNotReturned()
    {
        // Arrange
        var learning = await AddLearning("1111111111", approvalsId: 1, ukprn: Ukprn + 1);

        // Act
        var result = await _sut.Handle(QueryFor(learning));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenApprenticeshipsAreNotReturnedWhenAllEpisodesAreRemoved()
    {
        // Arrange
        var learning = await AddLearning("1111111111", approvalsId: 1, isRemoved: true);

        // Act
        var result = await _sut.Handle(QueryFor(learning));

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenRemovedEpisodesAreExcludedFromApprenticeship()
    {
        // Arrange
        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner
        {
            Key = learnerKey, Uln = "1111111111",
            FirstName = "Jane", LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1)
        });

        var learning = BuildActiveApprenticeship(Ukprn, learnerKey, approvalsId: 1);

        var removedEpisode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = Ukprn,
            TrainingCode = "ST0001",
            LegalEntityName = "Test Employer",
            IsRemoved = true
        };
        learning.Episodes.Add(removedEpisode);

        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(QueryFor(learning));

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.Single().Episodes.Should().HaveCount(1);
        result.Items.Single().Episodes.Should().NotContain(x => x.Key == removedEpisode.Key);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoApprenticeshipsExist()
    {
        // Act
        var result = await _sut.Handle(new GetLearningsWithEpisodesRequest { Ukprn = Ukprn, LearningKeys = [Guid.NewGuid()] });

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoKeysAreRequested()
    {
        // Arrange
        await AddLearning("1111111111", approvalsId: 1);

        // Act
        var result = await _sut.Handle(new GetLearningsWithEpisodesRequest { Ukprn = Ukprn, LearningKeys = [] });

        // Assert
        result.Should().BeNull();
    }

    private static GetLearningsWithEpisodesRequest QueryFor(params ApprenticeshipLearning[] learnings)
    {
        return new GetLearningsWithEpisodesRequest { Ukprn = Ukprn, LearningKeys = learnings.Select(x => x.Key).ToList() };
    }

    private async Task<ApprenticeshipLearning> AddLearning(string uln, int approvalsId, long ukprn = Ukprn, bool isRemoved = false)
    {
        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner
        {
            Key = learnerKey, Uln = uln,
            FirstName = "Jane", LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1)
        });

        var learning = BuildActiveApprenticeship(ukprn, learnerKey, approvalsId);
        learning.Episodes.First().IsRemoved = isRemoved;
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();
        return learning;
    }

    private static ApprenticeshipLearning BuildActiveApprenticeship(long ukPrn, Guid learnerKey, int approvalsId)
    {
        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), TrainingCode = "ST0001" };
        learning.LearnerKey = learnerKey;

        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukPrn,
            LegalEntityName = "Test Employer",
            ApprovalsApprenticeshipId = approvalsId
        };
        // Price active during academic year 2020/21
        episode.Prices.Add(new EpisodePrice
        {
            Key = Guid.NewGuid(),
            StartDate = new DateTime(2020, 8, 1),
            EndDate = new DateTime(2021, 7, 31),
            TotalPrice = 10000
        });
        learning.Episodes.Add(episode);
        return learning;
    }
}
