using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipRepository;

[TestFixture]
public class WhenGettingUnapprovedApprenticeshipLearning
{
    private LearningDataContext _dbContext = null!;
    private ApprenticeshipLearningRepository _sut = null!;
    private const string Uln = "44444444";
    private const string TrainingCode = "21";
    private readonly Guid _learnerKey = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);

        _dbContext.LearnersDbSet.Add(new Learner { Key = _learnerKey, Uln = Uln, FirstName = "Alex", LastName = "Jones" });

        var episodeKey = Guid.NewGuid();
        var learning = new DataAccess.Entities.Learning.ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = _learnerKey, TrainingCode = TrainingCode };
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = episodeKey,
            Ukprn = 10005077,
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
        var learning = new DataAccess.Entities.Learning.ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = _learnerKey, TrainingCode = TrainingCode };
        var episodeKey = Guid.NewGuid();

        // Adding a second draft learning with the same learner key and training code, but a different provider (ukprn) to ensure that the repository correctly filters by ukprn when provided.
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = episodeKey,
            Ukprn = 10005078,
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
        await _dbContext.SaveChangesAsync();

        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: TrainingCode, ukprn: 10005078);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheDraftIsFoundByUlnAndTrainingCode_MultipleEpisodes_NoSingleEpisodeIsBothUnapprovedAndMatchesProvider_ReturnNull()
    {
        var learning = new DataAccess.Entities.Learning.ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = _learnerKey, TrainingCode = TrainingCode };
        var episodeKey = Guid.NewGuid();
        var secondEpisodeKey = Guid.NewGuid();

        // Adding a second draft learning with the same learner key and training code, but a different provider (ukprn) to ensure that the repository correctly filters by ukprn when provided.
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = episodeKey,
            Ukprn = 10005078,
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
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = secondEpisodeKey,
            Ukprn = 10005079,
            IsApproved = true,
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
        await _dbContext.SaveChangesAsync();

        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: TrainingCode, ukprn: 10005079);

        result.Should().BeNull();
    }

    [Test]
    public async Task ThenTheDraftIsFoundByUlnAndTrainingCodeAndUkprn_EvenThoughTheRealApprovalsApprenticeshipIdDiffersFromTheDraftsZero()
    {
        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: TrainingCode, ukprn: 10005077);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AndTrainingCodeDoesNotMatch_ThenNullIsReturned()
    {
        ILearningRepository repository = _sut;

        var result = await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: "SOME-OTHER-CODE");

        result.Should().BeNull();
    }

    [Test]
    public async Task AndTheReturnedLearningIsApprovedAndSaved_ThenTheChangeIsPersisted()
    {
        ILearningRepository repository = _sut;
        var learning = (ApprenticeshipLearningDomainModel)(await repository.GetUnapprovedLearning(Uln, apprenticeshipId: 200003, trainingCode: TrainingCode, ukprn: 10005077))!;

        learning.Approve(employerAccountId: 1, EmployerType.NonLevy, fundingEmployerAccountId: null, legalEntityName: "Test Org", approvalsApprenticeshipId: 200003);

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var persisted = await _dbContext.ApprenticeshipLearningDbSet
            .Include(x => x.Episodes)
            .Where(x => x.LearnerKey == learning.LearnerKey)
            .SelectMany(x => x.Episodes)
            .SingleAsync();

        persisted.IsApproved.Should().BeTrue();
        persisted.ApprovalsApprenticeshipId.Should().Be(200003);
    }
}
