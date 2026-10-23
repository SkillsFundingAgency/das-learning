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
    public void AndSameWithdrawalDateAndReasonProvided_ThenNoChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, domainModel.LatestEpisode.WithdrawalReason);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        result.Should().NotContain(x => x == LearningUpdateChanges.WithdrawalReasonChanged);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(withdrawalDate);
        domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndOnlyWithdrawalReasonChanged_ThenReasonUpdatedAndEventRaised()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate);
        var newReasonCode = (short)(domainModel.LatestEpisode.WithdrawalReason!.Value == 43 ? 44 : 43);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, newReasonCode);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.WithdrawalReasonChanged);
        result.Should().NotContain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(withdrawalDate);
        domainModel.Episodes.First().WithdrawalReason.Should().Be(newReasonCode);

        var withdrawnEvent = domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Single();
        withdrawnEvent.WithdrawalReasonCode.Should().Be(newReasonCode);
        withdrawnEvent.LastDayOfLearning.Should().Be(withdrawalDate);
    }

    [Test]
    public void AndOnlyWithdrawalReasonChangedOnUnapprovedEpisode_ThenReasonUpdatedAndNoEventRaised()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate, isApproved: false);
        var newReasonCode = (short)(domainModel.LatestEpisode.WithdrawalReason!.Value == 43 ? 44 : 43);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, newReasonCode);

        //Act
        domainModel.Update(updateModel);

        //Assert
        domainModel.Episodes.First().WithdrawalReason.Should().Be(newReasonCode);
        domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
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
        domainModel.Episodes.First().WithdrawalReason.Should().BeNull();
        domainModel.FlushEvents().Should().ContainEquivalentOf(new WithdrawalRevertedEvent
        {
            ApprovalsApprenticeshipId = domainModel.LatestEpisode.ApprovalsApprenticeshipId,
            LearningKey = domainModel.Key
        });
    }

    [Test]
    public void AndWithdrawalDateProvided_ThenChangeMade()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var withdrawalReasonCode = _fixture.Create<short>();
        var domainModel = GetLearningDomainModel(withdrawalDate.AddDays(30));
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, withdrawalReasonCode);

        //Act
        var result = domainModel.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.Withdrawal);
        domainModel.Episodes.First().WithdrawalDate.Should().Be(withdrawalDate);
        domainModel.Episodes.First().WithdrawalReason.Should().Be(withdrawalReasonCode);

        var withdrawnEvent = domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Single();
        withdrawnEvent.WithdrawalReasonCode.Should().Be(withdrawalReasonCode);
        withdrawnEvent.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Test]
    public void AndWithdrawalDateProvidedWithoutReason_ThenReasonIsNullAndEventReasonIsZero()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var domainModel = GetLearningDomainModel(withdrawalDate.AddDays(30));
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, null);

        //Act
        domainModel.Update(updateModel);

        //Assert
        domainModel.Episodes.First().WithdrawalReason.Should().BeNull();
        domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Single().WithdrawalReasonCode.Should().Be(0);
    }

    [Test]
    public void AndEpisodeIsNotApproved_ThenReasonIsStoredAndNoEventRaised()
    {
        //Arrange
        var withdrawalDate = _fixture.Create<DateTime>();
        var withdrawalReasonCode = _fixture.Create<short>();
        var domainModel = GetLearningDomainModel(null, isApproved: false);
        var updateModel = GetLearnerUpdateModel(domainModel, withdrawalDate, withdrawalReasonCode);

        //Act
        domainModel.Update(updateModel);

        //Assert
        domainModel.Episodes.First().WithdrawalReason.Should().Be(withdrawalReasonCode);
        domainModel.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
    }

    private LearnerDomainModel GetLearnerDomainModel()
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel GetLearningDomainModel(DateTime? withdrawalDate, bool isApproved = true)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();

        episode.WithdrawalDate = withdrawalDate;
        episode.WithdrawalReason = withdrawalDate.HasValue ? _fixture.Create<short>() : null;
        episode.IsApproved = isApproved;

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        return ApprenticeshipLearningDomainModel.Get(entity);
    }

    private LearningUpdateContext GetLearnerUpdateModel(ApprenticeshipLearningDomainModel domainModel, DateTime? withdrawalDate, short? withdrawalReasonCode = null)
    {
        var learnerDomainModel = GetLearnerDomainModel();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(domainModel.GetEntity(), learnerDomainModel.GetEntity());
        updateModel.OnProgrammeDetails.WithdrawalDate = withdrawalDate;
        updateModel.OnProgrammeDetails.WithdrawalReasonCode = withdrawalReasonCode;
        return updateModel;
    }
}
