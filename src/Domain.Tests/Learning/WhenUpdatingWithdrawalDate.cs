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
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

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
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().LastDayOfLearning.Should().BeNull();
        domainModel.Episodes.First().LearningStatus.Should().Be(LearnerStatus.Active);
    }

    [Test]
    public void AndSameWithdrawalDateProvided_ThenNoChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().LastDayOfLearning.Should().Be(withdrawalDate);
        domainModel.Episodes.First().LearningStatus.Should().Be(LearnerStatus.Withdrawn);
    }

    [Test]
    public void AndWithdrawalRevered_ThenChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var updateModel = GetLearnerUpdateModel(domainModel, null);

        //Act
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.ReverseWithdrawal);
        domainModel.Episodes.First().LastDayOfLearning.Should().Be(null);
        domainModel.Episodes.First().LearningStatus.Should().Be(LearnerStatus.Active);
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
        var result = domainModel.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().LastDayOfLearning.Should().Be(withdrawalDate);
        domainModel.Episodes.First().LearningStatus.Should().Be(LearnerStatus.Withdrawn);
    }

    private LearningDomainModel GetLearningDomainModel(DateTime? withdrawalDate)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.Episode>();

        episode.LastDayOfLearning = withdrawalDate;
        if(withdrawalDate.HasValue)
        {
            episode.LearningStatus = LearnerStatus.Withdrawn.ToString();
        }
        else
        {
            episode.LearningStatus = LearnerStatus.Active.ToString();
        }

        entity.Episodes = new List<DataAccess.Entities.Learning.Episode> { episode };
        return LearningDomainModel.Get(entity);
    }

    private LearnerUpdateModel GetLearnerUpdateModel(LearningDomainModel domainModel, DateTime? withdrawalDate)
    {
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(domainModel.GetEntity());
        updateModel.Delivery.WithdrawalDate = withdrawalDate;
        return updateModel;
    }
}
