using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

public class WhenUpdatingWithdrawalDate
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndNoWithdrawalDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var domainModel = GetLearningDomainModel(null);
        var updateModel = GetLearnerUpdateModel(domainModel, null);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().BeNull();
    }

    [Test]
    public void AndSameWithdrawalDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(withdrawalDate);
    }

    [Test]
    public void AndWithdrawalRevered_ThenChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var updateModel = GetLearnerUpdateModel(domainModel, null);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.ReverseWithdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(null);
        domainModel.FlushEvents().Should().ContainEquivalentOf(new WithdrawalRevertedEvent
        {
            ApprovalsApprenticeshipId = domainModel.ApprovalsApprenticeshipId,
            LearningKey = domainModel.Key
        });
    }

    [Test]
    public void AndWithdrawalDateProvided_ThenChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate.AddDays(30));
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(withdrawalDate);
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(DateTime? withdrawalDate)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        episode.WithdrawalDate = withdrawalDate;

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private LearningUpdateContext GetLearnerUpdateModel(ApprenticeshipLearningDomainModel domainModel, DateTime? withdrawalDate)
    {
        var learnerDomainModel = GetLearnerDomainModel();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.Delivery.WithdrawalDate = withdrawalDate;
        return updateModel;
    }
}
