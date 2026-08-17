using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipRepository;

[TestFixture]
public class WhenGettingUnapprovedApprenticeshipLearning
{
    private LearningDataContext _dbContext;
    private ApprenticeshipLearningRepository _sut;
    private const string Uln = "44444444";
    private const string TrainingCode = "21";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = Uln, FirstName = "Alex", LastName = "Jones" });

        var episodeKey = Guid.NewGuid();
        var learning = new DataAccess.Entities.Learning.ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = learnerKey };
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = episodeKey,
            Ukprn = 10005077,
            TrainingCode = TrainingCode,
            IsApproved = false,
            ApprovalsApprenticeshipId = 0,
            LegalEntityName = string.Empty,
            EmployerType = EmployerType.NonLevy,
            Prices =
            [
                new EpisodePrice
                {
                    Key = Guid.NewGuid(),
                    EpisodeKey = episodeKey,
                    StartDate = new DateTime(2026, 4, 1),
                    EndDate = new DateTime(2028, 3, 31),
                    TotalPrice = 8000,
                    TrainingPrice = 6400,
                    EndPointAssessmentPrice = 1600
                }
            ]
        });
        _dbContext.Set<DataAccess.Entities.Learning.ApprenticeshipLearning>().Add(learning);
        _dbContext.SaveChanges();

        _sut = new ApprenticeshipLearningRepository(
            new Lazy<LearningDataContext>(() => _dbContext),
            new ApprenticeshipLearningFactory(),
            Mock.Of<IUnitOfWork>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenTheDraftIsFoundByUlnAndTrainingCode_EvenThoughTheRealApprovalsApprenticeshipIdDiffersFromTheDraftsZero()
    {
        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: TrainingCode);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AndTrainingCodeDoesNotMatch_ThenNullIsReturned()
    {
        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: "SOME-OTHER-CODE");

        result.Should().BeNull();
    }
}
