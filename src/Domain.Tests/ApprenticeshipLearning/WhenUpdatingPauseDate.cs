using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

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
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(null);
        var eventBuilder = new LearnerUpdatedEventBuilder(learnerDomainModel, learningDomainModel);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, null);

        //Act
        var result = learningDomainModel.UpdateLearnerDetails(updateModel, eventBuilder);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.BreakInLearningStarted ||
            x == LearningUpdateChanges.BreakInLearningRemoved);
        learningDomainModel.Episodes.First().PauseDate.Should().BeNull();
    }

    [Test]
    public void AndSamePauseDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(pauseDate);
        var eventBuilder = new LearnerUpdatedEventBuilder(learnerDomainModel, learningDomainModel);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, pauseDate);

        //Act
        var result = learningDomainModel.UpdateLearnerDetails(updateModel, eventBuilder);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.BreakInLearningStarted ||
            x == LearningUpdateChanges.BreakInLearningRemoved);
        learningDomainModel.Episodes.First().PauseDate.Should().Be(pauseDate);
    }

    [Test]
    public void AndPauseRemoved_ThenChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(pauseDate);
        var eventBuilder = new LearnerUpdatedEventBuilder(learnerDomainModel, learningDomainModel);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, null);

        //Act
        var result = learningDomainModel.UpdateLearnerDetails(updateModel, eventBuilder);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.BreakInLearningRemoved);
        learningDomainModel.Episodes.First().PauseDate.Should().BeNull();
    }

    [Test]
    public void AndPauseDateProvided_ThenChangeMade()
    {
        //Arrange
        var pauseDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(null);
        var eventBuilder = new LearnerUpdatedEventBuilder(learnerDomainModel, learningDomainModel);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, pauseDate);

        //Act
        var result = learningDomainModel.UpdateLearnerDetails(updateModel, eventBuilder);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.BreakInLearningStarted);
        learningDomainModel.Episodes.First().PauseDate.Should().Be(pauseDate);
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(DateTime? pauseDate)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        episode.PauseDate = pauseDate;

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private LearningUpdateContext GetLearnerUpdateModel(ApprenticeshipLearningDomainModel domainModel, LearnerDomainModel learnerDomainModel, DateTime? pauseDate)
    {
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.OnProgrammeDetails.PauseDate = pauseDate;
        return updateModel;
    }
}