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
    private LearningDataContext _dbContext;
    private GetLearningsWithEpisodesRequestQueryHandler _sut;

    // Academic year 2021 period 1 = last day of Aug 2021 (Aug 31, 2021)
    // Academic year start = Aug 1, 2021
    private static readonly GetLearningsWithEpisodesRequest DefaultQuery = new()
    {
        Ukprn = 1000,
        CollectionYear = 2021,
        CollectionPeriod = 1
    };

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
        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner
        {
            Key = learnerKey, Uln = "1111111111",
            FirstName = "Jane", LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1)
        });

        var learning = BuildActiveApprenticeship(DefaultQuery.Ukprn, learnerKey, approvalsId: 1);
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(DefaultQuery);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.Single().Uln.Should().Be("1111111111");
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoApprenticeshipsExist()
    {
        // Act
        var result = await _sut.Handle(DefaultQuery);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenPagedApprenticeshipsAreReturnedWhenPaginationSpecified()
    {
        // Arrange
        const int pageSize = 2;
        var query = new GetLearningsWithEpisodesRequest { Ukprn = 1000, CollectionYear = 2021, CollectionPeriod = 1, Page = 1, PageSize = pageSize };

        for (var i = 1; i <= 3; i++)
        {
            var learnerKey = Guid.NewGuid();
            _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = $"111111111{i}", FirstName = "A", LastName = "B", DateOfBirth = new DateTime(1990, 1, 1) });
            _dbContext.ApprenticeshipLearningDbSet.Add(BuildActiveApprenticeship(query.Ukprn, learnerKey, approvalsId: i));
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(pageSize);
        result.TotalItems.Should().Be(3);
        result.PageSize.Should().Be(pageSize);
        result.Page.Should().Be(1);
    }

    private static ApprenticeshipLearning BuildActiveApprenticeship(long ukPrn, Guid learnerKey, int approvalsId)
    {
        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid() };
        learning.LearnerKey = learnerKey;

        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukPrn,
            TrainingCode = "ST0001",
            FundingType = FundingType.Levy,
            FundingPlatform = FundingPlatform.DAS,
            LegalEntityName = "Test Employer",
            ApprovalsApprenticeshipId = approvalsId
        };
        // Price active during academic year 2020/21 (period 1 = Aug 31, 2020)
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
