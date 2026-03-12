using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Queries.GetLearningKeyByLearningId;

namespace SFA.DAS.Learning.Queries.UnitTests;

public class WhenGetLearningKeyById
{
    private LearningDataContext _dbContext;
    private GetLearningKeyByLearningIdQueryHandler _sut;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);
        _sut = new GetLearningKeyByLearningIdQueryHandler(_dbContext);
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenLearningKeyIsReturned()
    {
        // Arrange
        var learnerKey = Guid.NewGuid();
        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), ApprovalsApprenticeshipId = 42 };
        learning.LearnerKey = learnerKey;
        _dbContext.ApprenticeshipLearningDbSet.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetLearningKeyByLearningIdRequest { ApprenticeshipId = 42 };

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.LearningKey.Should().Be(learning.Key);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoMatchingLearning()
    {
        // Arrange
        var query = new GetLearningKeyByLearningIdRequest { ApprenticeshipId = 999 };

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.LearningKey.Should().BeNull();
    }
}
