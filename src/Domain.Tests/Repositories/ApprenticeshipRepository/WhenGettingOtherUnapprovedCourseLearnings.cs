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
public class WhenGettingOtherUnapprovedCourseLearnings
{
    private LearningDataContext _dbContext;
    private ApprenticeshipLearningRepository _sut;
    private Guid _learnerKey;
    private const long Ukprn = 10005077;
    private const string CurrentTrainingCode = "ST0002";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);

        _learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = _learnerKey, Uln = "44444444", FirstName = "Alex", LastName = "Jones" });
        _dbContext.SaveChanges();

        _sut = new ApprenticeshipLearningRepository(
            new Lazy<LearningDataContext>(() => _dbContext),
            new ApprenticeshipLearningFactory(),
            Mock.Of<IUnitOfWork>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    private void AddLearning(string trainingCode, bool isApproved, bool isRemoved, long ukprn = Ukprn)
    {
        var episodeKey = Guid.NewGuid();
        var learning = new DataAccess.Entities.Learning.ApprenticeshipLearning { Key = Guid.NewGuid(), LearnerKey = _learnerKey };
        learning.Episodes.Add(new ApprenticeshipEpisode
        {
            Key = episodeKey,
            Ukprn = ukprn,
            TrainingCode = trainingCode,
            IsApproved = isApproved,
            IsRemoved = isRemoved,
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
    }

    [Test]
    public async Task ThenAnUnapprovedLearningForADifferentCourseIsReturned()
    {
        AddLearning("ST0001", isApproved: false, isRemoved: false);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().ContainSingle();
    }

    [Test]
    public async Task AndTheOtherLearningIsForTheSameCourse_ThenItIsNotReturned()
    {
        AddLearning(CurrentTrainingCode, isApproved: false, isRemoved: false);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task AndTheOtherLearningIsAlreadyApproved_ThenItIsNotReturned()
    {
        AddLearning("ST0001", isApproved: true, isRemoved: false);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task AndTheOtherLearningIsAlreadyRemoved_ThenItIsNotReturned()
    {
        AddLearning("ST0001", isApproved: false, isRemoved: true);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task AndTheOtherLearningIsForADifferentUkprn_ThenItIsNotReturned()
    {
        AddLearning("ST0001", isApproved: false, isRemoved: false, ukprn: 99999999);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task AndMultipleUnapprovedCoursesExist_ThenAllAreReturned()
    {
        AddLearning("ST0001", isApproved: false, isRemoved: false);
        AddLearning("ST0003", isApproved: false, isRemoved: false);

        var result = await _sut.GetOtherUnapprovedCourseLearnings(_learnerKey, Ukprn, CurrentTrainingCode);

        result.Should().HaveCount(2);
    }
}
