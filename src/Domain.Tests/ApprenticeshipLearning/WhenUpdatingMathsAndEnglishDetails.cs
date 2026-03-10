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
public class WhenUpdatingMathsAndEnglishDetails
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
        MathsAndEnglishUpdateDetails newCourse = null;

        if (changed)
        {
            newCourse = _fixture.Create<MathsAndEnglishUpdateDetails>();
            updateModel.MathsAndEnglishCourses.Add(newCourse);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learningEntity.EnglishAndMathsCourses.Count.Should().Be(updateModel.MathsAndEnglishCourses.Count);
        if (changed)
        {
            learningEntity.EnglishAndMathsCourses.Should().Contain(x => x.Course == newCourse.Course);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
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
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.FirstOrDefault();

        if (changed) mathsAndEnglishUpdateModel.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learning.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).CompletionDate.GetValueOrDefault().Date.Should().Be(mathsAndEnglishUpdateModel.CompletionDate?.Date);
        learningEntity.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).CompletionDate.GetValueOrDefault().Date.Should().Be(mathsAndEnglishUpdateModel.CompletionDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
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
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.FirstOrDefault();
        mathsAndEnglishUpdateModel.WithdrawalDate = mathsAndEnglishUpdateModel.WithdrawalDate?.Date;

        if (changed) mathsAndEnglishUpdateModel.WithdrawalDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learning.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).WithdrawalDate.GetValueOrDefault().Date.Should().Be(mathsAndEnglishUpdateModel.WithdrawalDate?.Date);
        learningEntity.EnglishAndMathsCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).WithdrawalDate.GetValueOrDefault().Date.Should().Be(mathsAndEnglishUpdateModel.WithdrawalDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglishWithdrawal);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenStartDateIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses.ForEach(x => x.StartDate = x.StartDate.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);


        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed) mathsAndEnglishUpdateModel.StartDate = _fixture.Create<DateTime>().Date;

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).StartDate.Should().Be(mathsAndEnglishUpdateModel.StartDate.Date);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).StartDate.Should().Be(mathsAndEnglishUpdateModel.StartDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPlannedEndDateIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses.ForEach(x => x.PlannedEndDate = x.PlannedEndDate.Date);

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed) mathsAndEnglishUpdateModel.PlannedEndDate = _fixture.Create<DateTime>().Date;

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PlannedEndDate.Should().Be(mathsAndEnglishUpdateModel.PlannedEndDate.Date);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PlannedEndDate.Should().Be(mathsAndEnglishUpdateModel.PlannedEndDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPriorLearningPercentageIsUpdated(bool changed)
    {
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed)
        {
            var newValue = (mathsAndEnglishUpdateModel.PriorLearningPercentage ?? 0) + 10;
            mathsAndEnglishUpdateModel.PriorLearningPercentage = newValue;
        }

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PriorLearningPercentage.Should().Be(mathsAndEnglishUpdateModel.PriorLearningPercentage);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PriorLearningPercentage.Should().Be(mathsAndEnglishUpdateModel.PriorLearningPercentage);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenOldCoursesAreRemoved(bool changed)
    {
        //Arrange
        var course = _fixture.Create<MathsAndEnglishUpdateDetails>();

        (var learnerEntity, var learningEntity) = CreateEntities();

        learningEntity.EnglishAndMathsCourses = [new EnglishAndMaths { Course = course.Course, LearnAimRef = course.LearnAimRef }];

        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);

        if (changed)
        {
            updateModel.MathsAndEnglishCourses.RemoveAll(_ => true);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        if (changed)
        {
            learning.EnglishAndMathsCourses.Count.Should().Be(0);
            learningEntity.EnglishAndMathsCourses.Count.Should().Be(0);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
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
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed)
        {
            var newValue = mathsAndEnglishUpdateModel.Amount + 10;
            mathsAndEnglishUpdateModel.Amount = newValue;
        }

        var result = learning.Update(updateModel);

        learning.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).Amount.Should().Be(mathsAndEnglishUpdateModel.Amount);
        learningEntity.EnglishAndMathsCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).Amount.Should().Be(mathsAndEnglishUpdateModel.Amount);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenNewCourseWithWithdrawalReturnsCorrectChangeTypes(bool changed)
    {
        //Arrange
        (var learnerEntity, var learningEntity) = CreateEntities();
        (var learner, var learning) = CreateDomainObjects(learnerEntity, learningEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);
        MathsAndEnglishUpdateDetails newCourse = null;

        if (changed)
        {
            newCourse = _fixture.Create<MathsAndEnglishUpdateDetails>();
            newCourse.WithdrawalDate = _fixture.Create<DateTime>();
            updateModel.MathsAndEnglishCourses.Add(newCourse);
        }

        //Act
        var result = learning.Update(updateModel);

        //Assert
        learningEntity.EnglishAndMathsCourses.Count.Should().Be(updateModel.MathsAndEnglishCourses.Count);
        if (changed)
        {
            learningEntity.EnglishAndMathsCourses.Should().Contain(x => x.Course == newCourse.Course);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglishWithdrawal);
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