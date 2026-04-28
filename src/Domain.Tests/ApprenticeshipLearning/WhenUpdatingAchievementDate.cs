using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

public class WhenUpdatingAchievementDate
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndNoAchievementDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(null);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, null);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.AchievementDateChanged);
        learningDomainModel.AchievementDate.Should().BeNull();
    }

    [Test]
    public void AndSameAchievementDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var achievementDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(achievementDate);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, achievementDate);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.AchievementDateChanged);
        learningDomainModel.AchievementDate.Should().Be(achievementDate);
    }

    [Test]
    public void AndAchievementDateRemoved_ThenChangeMade()
    {
        //Arrange
        var achievementDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(achievementDate);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, null);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.AchievementDateChanged);
        learningDomainModel.AchievementDate.Should().BeNull();
    }

    [Test]
    public void AndAchievementDateProvided_ThenChangeMade()
    {
        //Arrange
        var achievementDate = _fixture.Create<DateTime>();
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(null);
        var updateModel = GetLearnerUpdateModel(learningDomainModel, learnerDomainModel, achievementDate);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.AchievementDateChanged);
        learningDomainModel.AchievementDate.Should().Be(achievementDate);
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(DateTime? achievementDate)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        entity.AchievementDate = achievementDate;

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private LearningUpdateContext GetLearnerUpdateModel(ApprenticeshipLearningDomainModel domainModel, LearnerDomainModel learnerDomainModel, DateTime? AchievementDate)
    {
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.OnProgrammeDetails.AchievementDate = AchievementDate;
        return updateModel;
    }
}