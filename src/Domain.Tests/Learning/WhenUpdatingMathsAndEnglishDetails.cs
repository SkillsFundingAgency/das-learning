using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

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
            result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMathsNewCourse);
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
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMathsCompletion);
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
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.EnglishAndMathsWithdrawal);
    }
}