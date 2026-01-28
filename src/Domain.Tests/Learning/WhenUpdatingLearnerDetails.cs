using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenUpdatingLearnerDetails
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenCompletionDateIsUpdated(bool changed)
    {
        //Arrange
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        entity.CompletionDate = entity.CompletionDate?.Date;
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);

        if (changed) updateModel.Learning.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        learning.CompletionDate.Should().Be(updateModel.Learning.CompletionDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.CompletionDate);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ThenDateOfBirthIsUpdated(bool changed)
    {
        // Arrange
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        entity.DateOfBirth = entity.DateOfBirth.Date; // normalize

        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);

        if (changed)
            updateModel.Learning.DateOfBirth = _fixture.Create<DateTime>();

        // Act
        var result = learning.UpdateLearnerDetails(updateModel);

        // Assert
        learning.DateOfBirth.Should().Be(updateModel.Learning.DateOfBirth);

        if (changed)
            result.Should().Contain(x => x == LearningUpdateChanges.DateOfBirthChanged);
    }
}