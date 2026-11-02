using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

public class WhenForcingEarningsSync
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndFlagIsSet_ThenForceEarningsSyncIsTheOnlyChange()
    {
        //Arrange
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(forceEarningsSync: true, isRemoved: false);
        var updateModel = GetUnchangedUpdateModel(learningDomainModel, learnerDomainModel);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().BeEquivalentTo([LearningUpdateChanges.ForceEarningsSync]);
    }

    [Test]
    public void AndFlagIsNotSet_ThenNoChangesAreReturned()
    {
        //Arrange
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(forceEarningsSync: false, isRemoved: false);
        var updateModel = GetUnchangedUpdateModel(learningDomainModel, learnerDomainModel);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void AndFlagIsSetOnARemovedEpisode_ThenReinstatedAndForceEarningsSyncAreReturned()
    {
        //Arrange
        var learnerDomainModel = GetLearnerDomainModel();
        var learningDomainModel = GetLearningDomainModel(forceEarningsSync: true, isRemoved: true);
        var updateModel = GetUnchangedUpdateModel(learningDomainModel, learnerDomainModel);

        //Act
        var result = learningDomainModel.Update(updateModel);

        //Assert
        result.Should().BeEquivalentTo([LearningUpdateChanges.Reinstated, LearningUpdateChanges.ForceEarningsSync]);
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(bool forceEarningsSync, bool isRemoved)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        episode.ForceEarningsSync = forceEarningsSync;
        episode.IsRemoved = isRemoved;

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private static LearningUpdateContext GetUnchangedUpdateModel(ApprenticeshipLearningDomainModel domainModel, LearnerDomainModel learnerDomainModel)
    {
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.OnProgrammeDetails.AchievementDate = domainModel.AchievementDate;
        return updateModel;
    }
}
