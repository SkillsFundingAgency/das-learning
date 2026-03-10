using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Queries.GetLearnings;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGetLearnings
{
    private LearningDataContext _dbContext;
    private GetLearningsQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetLearningsQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenApprenticeshipsAreReturned()
    {
        // Arrange
        const long ukPrn = 1000;

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = "1111111111", FirstName = "Jane", LastName = "Doe" });

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), ApprovalsApprenticeshipId = 1 };
        learning.LearnerKey = learnerKey;
        learning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", FundingType = FundingType.Levy, LegalEntityName = "Test" });
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsRequest(ukPrn, null);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Learnings.Should().HaveCount(1);
        var item = result.Learnings.Single();
        item.Uln.Should().Be("1111111111");
        item.FirstName.Should().Be("Jane");
        item.LastName.Should().Be("Doe");
    }

    [Test]
    public async Task ThenApprenticeshipsAreFilteredByFundingPlatform()
    {
        // Arrange
        const long ukPrn = 1000;

        var learnerKey1 = Guid.NewGuid();
        var learnerKey2 = Guid.NewGuid();
        _dbContext.LearnersDbSet.AddRange(
            new Learner { Key = learnerKey1, Uln = "111", FirstName = "A", LastName = "B" },
            new Learner { Key = learnerKey2, Uln = "222", FirstName = "C", LastName = "D" });

        var dasLearning = new ApprenticeshipLearning { Key = Guid.NewGuid(), ApprovalsApprenticeshipId = 1 };
        dasLearning.LearnerKey = learnerKey1;
        dasLearning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "A1", FundingType = FundingType.Levy, FundingPlatform = FundingPlatform.DAS, LegalEntityName = "Test" });

        var nonDasLearning = new ApprenticeshipLearning { Key = Guid.NewGuid(), ApprovalsApprenticeshipId = 2 };
        nonDasLearning.LearnerKey = learnerKey2;
        nonDasLearning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "A2", FundingType = FundingType.Levy, FundingPlatform = FundingPlatform.SLD, LegalEntityName = "Test" });

        _dbContext.ApprenticeshipLearningDbSet.AddRange(dasLearning, nonDasLearning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsRequest(ukPrn, FundingPlatform.DAS);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Learnings.Should().HaveCount(1);
        result.Learnings.Single().Uln.Should().Be("111");
    }
}
