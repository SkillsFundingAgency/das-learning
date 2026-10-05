using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

/// <summary>
/// An unapproved draft has no ApprovalsApprenticeshipId, so Approvals must not be told about changes to it.
/// State and the returned changes are still updated; only the approval-facing events are suppressed.
/// </summary>
[TestFixture]
public class WhenUpdatingAnUnapprovedDraft
{
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp() => _fixture = new Fixture();

    private ApprenticeshipLearningDomainModel CreateDraft(DateTime? withdrawalDate = null)
        => ApprenticeshipLearningDomainModel.CreateDraft(Guid.NewGuid(), 12345678, "ST0001", new DraftApprenticeshipDetails
        {
            Cost = new Cost { FromDate = new DateTime(2025, 8, 1), TrainingPrice = 1000, EpaoPrice = 200 },
            ExpectedEndDate = new DateTime(2026, 7, 31),
            WithdrawalDate = withdrawalDate
        });

    private LearningUpdateContext UpdateModelMatching(ApprenticeshipLearningDomainModel draft)
    {
        var learner = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        return LearningUpdateModelHelper.CreateUpdateModel(draft.GetEntity(), learner);
    }

    [Test]
    public void AndAWithdrawalDateIsAdded_ThenTheEpisodeIsWithdrawnButNoWithdrawnEventIsRaised()
    {
        var draft = CreateDraft();
        var updateModel = UpdateModelMatching(draft);
        updateModel.Delivery.WithdrawalDate = new DateTime(2025, 10, 1);

        var changes = draft.Update(updateModel);

        changes.Should().Contain(LearningUpdateChanges.Withdrawal);
        draft.LatestEpisode.WithdrawalDate.Should().Be(new DateTime(2025, 10, 1));
        draft.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndAWithdrawalIsReversed_ThenTheWithdrawalIsClearedButNoRevertedEventIsRaised()
    {
        var draft = CreateDraft(withdrawalDate: new DateTime(2025, 10, 1));
        var updateModel = UpdateModelMatching(draft);
        updateModel.Delivery.WithdrawalDate = null;

        var changes = draft.Update(updateModel);

        changes.Should().Contain(LearningUpdateChanges.ReverseWithdrawal);
        draft.LatestEpisode.WithdrawalDate.Should().BeNull();
        draft.FlushEvents().OfType<WithdrawalRevertedEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndTheExpectedEndDateChanges_ThenThePriceEndDateIsUpdatedButNoEndDateChangedEventIsRaised()
    {
        var draft = CreateDraft();
        var updateModel = UpdateModelMatching(draft);
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2026, 9, 30);

        var changes = draft.Update(updateModel);

        changes.Should().Contain(LearningUpdateChanges.ExpectedEndDate);
        draft.EndDate.Should().Be(new DateTime(2026, 9, 30));
        draft.FlushEvents().OfType<EndDateChangedEvent>().Should().BeEmpty();
    }
}
