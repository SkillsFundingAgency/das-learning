using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenUpdatingLearningDetails
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
        var learnerEntity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        var learningEntity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        learningEntity.CompletionDate = learningEntity.CompletionDate?.Date;
        learningEntity.LearnerKey = learnerEntity.Key;

        var learner = LearnerDomainModel.Get(learnerEntity);
        var learning = ApprenticeshipLearningDomainModel.Get(learningEntity);
        var eventBuilder = new LearnerUpdatedEventBuilder(learner, learning);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learningEntity, learnerEntity);

        if (changed) updateModel.Learning.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel, eventBuilder);

        //Assert
        learning.CompletionDate.Should().Be(updateModel.Learning.CompletionDate?.Date);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.CompletionDate);
    }

}