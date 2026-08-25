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

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid() };
        learning.LearnerKey = learnerKey;
        learning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", LegalEntityName = "Test", ApprovalsApprenticeshipId = 1, IsApproved = true });
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsRequest(ukPrn);

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
    public async Task ThenDraftApprenticeshipsAreExcluded()
    {
        // Arrange
        const long ukPrn = 1000;

        var approvedLearnerKey = Guid.NewGuid();
        var draftLearnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.AddRange(
            new Learner { Key = approvedLearnerKey, Uln = "111", FirstName = "Approved", LastName = "Learner" },
            new Learner { Key = draftLearnerKey, Uln = "222", FirstName = "Draft", LastName = "Learner" });

        var approvedLearning = new ApprenticeshipLearning { Key = Guid.NewGuid() };
        approvedLearning.LearnerKey = approvedLearnerKey;
        approvedLearning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "A1", LegalEntityName = "Test", ApprovalsApprenticeshipId = 1, IsApproved = true });

        var draftLearning = new ApprenticeshipLearning { Key = Guid.NewGuid() };
        draftLearning.LearnerKey = draftLearnerKey;
        draftLearning.Episodes.Add(new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "A2", LegalEntityName = "Test", ApprovalsApprenticeshipId = 0, IsApproved = false });

        _dbContext.ApprenticeshipLearningDbSet.AddRange(approvedLearning, draftLearning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningsRequest(ukPrn);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Learnings.Should().HaveCount(1);
        result.Learnings.Single().Uln.Should().Be("111");
    }
}
