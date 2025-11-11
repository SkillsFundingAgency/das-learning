using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

public class WhenUpdatingPauseDate
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndNoPauseDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var domainModel = GetLearningDomainModel(null);
        var updateModel = GetLearnerUpdateModel(domainModel, null);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.BreakInLearningStarted ||
            x == LearningUpdateChanges.BreakInLearningRemoved);
        domainModel.Episodes.First().PauseDate.Should().BeNull();
    }

    [Test]
    public void AndSamePauseDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(pauseDate);
        var updateModel = GetLearnerUpdateModel(domainModel, pauseDate);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.BreakInLearningStarted ||
            x == LearningUpdateChanges.BreakInLearningRemoved);
        domainModel.Episodes.First().PauseDate.Should().Be(pauseDate);
    }

    [Test]
    public void AndPauseRemoved_ThenChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(pauseDate);
        var updateModel = GetLearnerUpdateModel(domainModel, null);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.BreakInLearningRemoved);
        domainModel.Episodes.First().PauseDate.Should().BeNull();
    }

    [Test]
    public void AndPauseDateProvided_ThenChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(null);
        var updateModel = GetLearnerUpdateModel(domainModel, pauseDate);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.BreakInLearningStarted);
        domainModel.Episodes.First().PauseDate.Should().Be(pauseDate);
    }

    private LearningDomainModel GetLearningDomainModel(DateTime? pauseDate)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.Episode>();

        episode.PauseDate = pauseDate;

        entity.Episodes = new List<DataAccess.Entities.Learning.Episode> { episode };
        return LearningDomainModel.Get(entity);
    }

    private LearnerUpdateModel GetLearnerUpdateModel(LearningDomainModel domainModel, DateTime? pauseDate)
    {
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(domainModel.GetEntity());
        updateModel.OnProgrammeDetails.PauseDate = pauseDate;
        return updateModel;
    }
}