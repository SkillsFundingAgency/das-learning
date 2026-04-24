using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
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

        var learning = new ApprenticeshipLearning { Key = Guid.NewGuid(), ApprovalsApprenticeshipId = 1 };
        learning.LearnerKey = learnerKey;
        var episode = new ApprenticeshipEpisode { Key = Guid.NewGuid(), Ukprn = ukPrn, TrainingCode = "123", FundingType = FundingType.Levy, LegalEntityName = "Test" };
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
        result.Items.Single().Key.Should().Be(learning.Key);
    }

}
