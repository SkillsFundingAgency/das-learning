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
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var learning = LearningDomainModel.Get(entity);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(entity);

        if(changed) updateModel.Learning.CompletionDate = _fixture.Create<DateTime>();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        learning.CompletionDate.Should().Be(updateModel.Learning.CompletionDate);
        if (changed) result.Should().Contain(x => x == LearningUpdateChanges.CompletionDate);
    }
}