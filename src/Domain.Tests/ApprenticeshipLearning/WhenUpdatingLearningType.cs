using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

public class WhenUpdatingLearningType
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndLearningTypeIsUnchanged_ThenLearningTypeRemainsTheSame()
    {
        //Arrange
        var domainModel = GetLearningDomainModel(LearningType.Apprenticeship);
        var updateModel = GetLearnerUpdateModel(domainModel, LearningType.Apprenticeship);

        //Act
        domainModel.Update(updateModel);

        //Assert
        domainModel.LearningType.Should().Be(LearningType.Apprenticeship);
    }

    [Test]
    public void AndLearningTypeHasChanged_ThenLearningTypeIsUpdated()
    {
        //Arrange
        var domainModel = GetLearningDomainModel(LearningType.Apprenticeship);
        var updateModel = GetLearnerUpdateModel(domainModel, LearningType.FoundationApprenticeship);

        //Act
        domainModel.Update(updateModel);

        //Assert
        domainModel.LearningType.Should().Be(LearningType.FoundationApprenticeship);
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(LearningType learningType)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        entity.LearningType = learningType;
        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private LearningUpdateContext GetLearnerUpdateModel(ApprenticeshipLearningDomainModel domainModel, LearningType learningType)
    {
        var learnerDomainModel = GetLearnerDomainModel();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.Delivery.LearningType = learningType;
        return updateModel;
    }
}
