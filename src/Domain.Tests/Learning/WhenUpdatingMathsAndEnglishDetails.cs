using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

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
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        MathsAndEnglishUpdateDetails newCourse = null;

        if (changed)
        {
            newCourse = _fixture.Create<MathsAndEnglishUpdateDetails>();
            updateModel.MathsAndEnglishCourses.Add(newCourse);
        }

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        entity.MathsAndEnglishCourses.Count.Should().Be(updateModel.MathsAndEnglishCourses.Count);
        if (changed)
        {
            entity.MathsAndEnglishCourses.Should().Contain(x => x.Course == newCourse.Course);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenCompletionDateIsUpdated(bool changed)
    {
        //Arrange
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.MathsAndEnglishCourses.ForEach(x => x.CompletionDate = x.CompletionDate?.Date);
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.FirstOrDefault();

        if (changed) mathsAndEnglishUpdateModel.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        learning.MathsAndEnglishCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).CompletionDate.Should().Be(mathsAndEnglishUpdateModel.CompletionDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenWithdrawalDateIsUpdated(bool changed)
    {
        //Arrange
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.MathsAndEnglishCourses.ForEach(x => x.WithdrawalDate = x.WithdrawalDate?.Date);
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.FirstOrDefault();
        mathsAndEnglishUpdateModel.WithdrawalDate = mathsAndEnglishUpdateModel.WithdrawalDate?.Date;

        if (changed) mathsAndEnglishUpdateModel.WithdrawalDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        learning.MathsAndEnglishCourses.FirstOrDefault(x => x.Course == mathsAndEnglishUpdateModel.Course).WithdrawalDate.Should().Be(mathsAndEnglishUpdateModel.WithdrawalDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenStartDateIsUpdated(bool changed)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.MathsAndEnglishCourses.ForEach(x => x.StartDate = x.StartDate.Date);
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed) mathsAndEnglishUpdateModel.StartDate = _fixture.Create<DateTime>().Date;

        var result = learning.UpdateLearnerDetails(updateModel);

        learning.MathsAndEnglishCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).StartDate.Should().Be(mathsAndEnglishUpdateModel.StartDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPlannedEndDateIsUpdated(bool changed)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.MathsAndEnglishCourses.ForEach(x => x.PlannedEndDate = x.PlannedEndDate.Date);
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed) mathsAndEnglishUpdateModel.PlannedEndDate = _fixture.Create<DateTime>().Date;

        var result = learning.UpdateLearnerDetails(updateModel);

        learning.MathsAndEnglishCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PlannedEndDate.Should().Be(mathsAndEnglishUpdateModel.PlannedEndDate.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenPriorLearningPercentageIsUpdated(bool changed)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed)
        {
            var newValue = (mathsAndEnglishUpdateModel.PriorLearningPercentage ?? 0) + 10;
            mathsAndEnglishUpdateModel.PriorLearningPercentage = newValue;
        }

        var result = learning.UpdateLearnerDetails(updateModel);

        learning.MathsAndEnglishCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).PriorLearningPercentage.Should().Be(mathsAndEnglishUpdateModel.PriorLearningPercentage);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenOldCoursesAreRemoved(bool changed)
    {
        //Arrange
        var course = _fixture.Create<MathsAndEnglishUpdateDetails>();
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.MathsAndEnglishCourses = [new MathsAndEnglish { Course = course.Course }];

        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);

        if (changed)
        {
            updateModel.MathsAndEnglishCourses.RemoveAll(_ => true);
        }

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        if (changed)
        {
            entity.MathsAndEnglishCourses.Count.Should().Be(0);
            result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
        }
        else
        {
            entity.MathsAndEnglishCourses.Should().Contain(x => x.Course == course.Course);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenAmountIsUpdated(bool changed)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);
        var mathsAndEnglishUpdateModel = updateModel.MathsAndEnglishCourses.First();

        if (changed)
        {
            var newValue = mathsAndEnglishUpdateModel.Amount + 10;
            mathsAndEnglishUpdateModel.Amount = newValue;
        }

        var result = learning.UpdateLearnerDetails(updateModel);

        learning.MathsAndEnglishCourses.First(x => x.Course == mathsAndEnglishUpdateModel.Course).Amount.Should().Be(mathsAndEnglishUpdateModel.Amount);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.MathsAndEnglish);
    }
}