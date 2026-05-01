using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenUpdatingEnglishAndMathsDetails
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenNewCourseIsAdded(bool changed)
    {
        //Arrange
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        EnglishAndMathsUpdateDetails newCourse = null;

        if (changed)
        {
            newCourse = _fixture.Create<EnglishAndMathsUpdateDetails>();
            updateModel.EnglishAndMathsCourses.Add(newCourse);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learningEntity.EnglishAndMathsCourses.Count.Should().Be(updateModel.EnglishAndMathsCourses.Count);
        if (changed)
        {
            learningEntity.EnglishAndMathsCourses.Should().Contain(x => x.Course == newCourse.Course);
            result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenCompletionDateIsUpdated(bool changed)
    {
        //Arrange
        (var learnerEntity, var learningEntity) = CreateEntities();
        learningEntity.EnglishAndMathsCourses.ForEach(x => x.CompletionDate = x.CompletionDate?.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.FirstOrDefault();

        if (changed) englishAndMathsUpdateModel.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learning.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == englishAndMathsUpdateModel.Course).CompletionDate.GetValueOrDefault().Date.Should().Be(englishAndMathsUpdateModel.CompletionDate?.Date);
        learningEntity.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == englishAndMathsUpdateModel.Course).CompletionDate.GetValueOrDefault().Date.Should().Be(englishAndMathsUpdateModel.CompletionDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenWithdrawalDateIsUpdated(bool changed)
    {
        //Arrange
        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses.ForEach(x => x.WithdrawalDate = x.WithdrawalDate?.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.FirstOrDefault();
        englishAndMathsUpdateModel.WithdrawalDate = englishAndMathsUpdateModel.WithdrawalDate?.Date;

        if (changed) englishAndMathsUpdateModel.WithdrawalDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learning.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == englishAndMathsUpdateModel.Course).WithdrawalDate.GetValueOrDefault().Date.Should().Be(englishAndMathsUpdateModel.WithdrawalDate?.Date);
        learningEntity.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == englishAndMathsUpdateModel.Course).WithdrawalDate.GetValueOrDefault().Date.Should().Be(englishAndMathsUpdateModel.WithdrawalDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMathsWithdrawal);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenStartDateIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses.ForEach(x => x.StartDate = x.StartDate.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);


        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.First();

        if (changed) englishAndMathsUpdateModel.StartDate = _fixture.Create<DateTime>().Date;

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).StartDate.Should().Be(englishAndMathsUpdateModel.StartDate.Date);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).StartDate.Should().Be(englishAndMathsUpdateModel.StartDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPlannedEndDateIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses.ForEach(x => x.PlannedEndDate = x.PlannedEndDate.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.First();

        if (changed) englishAndMathsUpdateModel.PlannedEndDate = _fixture.Create<DateTime>().Date;

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).PlannedEndDate.Should().Be(englishAndMathsUpdateModel.PlannedEndDate.Date);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).PlannedEndDate.Should().Be(englishAndMathsUpdateModel.PlannedEndDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPriorLearningPercentageIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.First();

        if (changed)
        {
            var newValue = (englishAndMathsUpdateModel.CombinedFundingAdjustmentPercentage ?? 0) + 10;
            englishAndMathsUpdateModel.CombinedFundingAdjustmentPercentage = newValue;
        }

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).CombinedFundingAdjustmentPercentage.Should().Be(englishAndMathsUpdateModel.CombinedFundingAdjustmentPercentage);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).CombinedFundingAdjustmentPercentage.Should().Be(englishAndMathsUpdateModel.CombinedFundingAdjustmentPercentage);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenOldCoursesAreRemoved(bool changed)
    {
        //Arrange
        var course = _fixture.Create<EnglishAndMathsUpdateDetails>();

        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses = [new EnglishAndMaths { Course = course.Course, LearnAimRef = course.LearnAimRef }];

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);

        if (changed)
        {
            updateModel.EnglishAndMathsCourses.RemoveAll(_ => true);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        if (changed)
        {
            learning.EnglishAndMathsCourses.Count.Should().Be(0);
            learningEntity.EnglishAndMathsCourses.Count.Should().Be(0);
            result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
        }
        else
        {
            learning.EnglishAndMathsCourses.Should().Contain(x => x.Course == course.Course);
            learningEntity.EnglishAndMathsCourses.Should().Contain(x => x.Course == course.Course);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenAmountIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var englishAndMathsUpdateModel = updateModel.EnglishAndMathsCourses.First();

        if (changed)
        {
            var newValue = englishAndMathsUpdateModel.Amount + 10;
            englishAndMathsUpdateModel.Amount = newValue;
        }

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).Amount.Should().Be(englishAndMathsUpdateModel.Amount);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == englishAndMathsUpdateModel.Course).Amount.Should().Be(englishAndMathsUpdateModel.Amount);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenNewCourseWithWithdrawalReturnsCorrectChangeTypes(bool changed)
    {
        //Arrange
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        EnglishAndMathsUpdateDetails newCourse = null;

        if (changed)
        {
            newCourse = _fixture.Create<EnglishAndMathsUpdateDetails>();
            newCourse.WithdrawalDate = _fixture.Create<DateTime>();
            updateModel.EnglishAndMathsCourses.Add(newCourse);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learningEntity.EnglishAndMathsCourses.Count.Should().Be(updateModel.EnglishAndMathsCourses.Count);
        if (changed)
        {
            learningEntity.EnglishAndMathsCourses.Should().Contain(x => x.Course == newCourse.Course);
            result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMaths);
            result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMathsWithdrawal);
        }
    }

    private (DataAccess.Entities.Learning.Learner, DataAccess.Entities.Learning.ApprenticeshipLearning) CreateEntities()
    {
        var learnerEntity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        var learningEntity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        return (learnerEntity, learningEntity);
    }

    private (LearnerDomainModel, ApprenticeshipLearningDomainModel) CreateDomainObjects(
        DataAccess.Entities.Learning.Learner learnerEntity,
        DataAccess.Entities.Learning.ApprenticeshipLearning learningEntity
        )
    {
        var learning = ApprenticeshipLearningDomainModel.Get(learningEntity);
        var learner = LearnerDomainModel.Get(learnerEntity);

        return (learner, learning);
    }
}