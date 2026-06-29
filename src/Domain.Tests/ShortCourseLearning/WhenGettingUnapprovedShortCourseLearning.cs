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

namespace SFA.DAS.Learning.Domain.UnitTests.ShortCourseLearning;

[TestFixture]
public class WhenGettingUnapprovedShortCourseLearning
{
    private LearningDataContext _dbContext;
    private ShortCourseLearningRepository _sut;
    private const string Uln = "23456734";
    private const string ActualTrainingCode = "ZSC00001";

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<LearningDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LearningDataContext(options);

        var learnerKey = Guid.NewGuid();
        _dbContext.LearnersDbSet.Add(new Learner { Key = learnerKey, Uln = Uln, FirstName = "John", LastName = "Smith" });

        var learning = new DataAccess.Entities.Learning.ShortCourseLearning { Key = Guid.NewGuid(), LearnerKey = learnerKey, TrainingCode = ActualTrainingCode };
        learning.Episodes.Add(new ShortCourseEpisode { Key = Guid.NewGuid(), Ukprn = 10005077, TrainingCode = ActualTrainingCode, LearnerRef = "string", IsApproved = false });
        _dbContext.Set<DataAccess.Entities.Learning.ShortCourseLearning>().Add(learning);
        _dbContext.SaveChanges();

        _sut = new ShortCourseLearningRepository(
            new Lazy<LearningDataContext>(() => _dbContext),
            new ShortCourseLearningFactory(),
            Mock.Of<IUnitOfWork>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task ThenNullIsReturnedRatherThanThrowing_WhenTrainingCodeDoesNotMatch()
    {
        // Approval events for a different (mismatched) TrainingCode should not find this learning
        var result = await _sut.Get(Uln, unapprovedOnly: true, trainingCode: "SOME-OTHER-CODE");

        result.Should().BeNull();
    }
}
