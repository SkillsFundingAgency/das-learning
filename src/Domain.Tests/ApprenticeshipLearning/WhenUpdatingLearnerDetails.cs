using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

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
    public void ThenDateOfBirthIsUpdated(bool changed)
    {
        // Arrange
        var learnerEntity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        var learningEntity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        learnerEntity.DateOfBirth = learnerEntity.DateOfBirth.Date; // normalize
        learningEntity.LearnerKey = learnerEntity.Key;

        var learner = LearnerDomainModel.Get(learnerEntity);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);

        if (changed)
            updateModel.Learner.DateOfBirth = _fixture.Create<DateTime>();

        // Act
        var result = learner.Update(updateModel);

        // Assert
        learnerEntity.DateOfBirth.Should().Be(updateModel.Learner.DateOfBirth);

        if (changed)
            result.Should().Contain(x => x == LearningUpdateChanges.DateOfBirthChanged);
    }
}