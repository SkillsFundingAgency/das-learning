using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenAStartDateChangeIsApproved
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ByProviderThenTheStartDateChangeRecordIsUpdated()
    {
        //Arrange
        var approverUserId = _fixture.Create<string>();
        var apprenticeship = ApprenticeshipDomainModelTestHelper.BuildApprenticeshipWithPendingStartDateChange(pendingProviderApproval: true);

        //Act
        var now = DateTime.UtcNow;
        apprenticeship.ApproveStartDateChange(approverUserId);

        //Assert
        var entity = apprenticeship.GetEntity();
        entity.StartDateChanges.Any(x => x.RequestStatus == ChangeRequestStatus.Created).Should().BeFalse();
        var startDateChange = entity.StartDateChanges.Single(x => x.RequestStatus == ChangeRequestStatus.Approved);
        startDateChange.Should().NotBeNull();
        startDateChange.ProviderApprovedBy.Should().Be(approverUserId);
        startDateChange.ProviderApprovedDate.Should().BeAfter(now);
        startDateChange.EmployerApprovedBy.Should().NotBeNull();
        startDateChange.EmployerApprovedDate.Should().NotBeNull();
    }

    [Test]
    public void ByEmployerThenTheStartDateChangeRecordIsUpdated()
    {
        //Arrange
        var approverUserId = _fixture.Create<string>();
        var apprenticeship = ApprenticeshipDomainModelTestHelper.BuildApprenticeshipWithPendingStartDateChange(pendingProviderApproval: false);

        //Act
        var now = DateTime.UtcNow;
        apprenticeship.ApproveStartDateChange(approverUserId);

        //Assert
        var entity = apprenticeship.GetEntity();
        entity.StartDateChanges.Any(x => x.RequestStatus == ChangeRequestStatus.Created).Should().BeFalse();
        var startDateChange = entity.StartDateChanges.Single(x => x.RequestStatus == ChangeRequestStatus.Approved);
        startDateChange.Should().NotBeNull();
        startDateChange.EmployerApprovedBy.Should().Be(approverUserId);
        startDateChange.EmployerApprovedDate.Should().BeAfter(now);
        startDateChange.ProviderApprovedBy.Should().NotBeNull();
        startDateChange.ProviderApprovedDate.Should().NotBeNull();
    }

    //TODO start date change - Add unit tests for the correct handing of episodes and prices

    [Test]
    public void ToEarlierDateThenExistingEpisodePriceIsOverwritten()
    {
        //Arrange
        var approverUserId = _fixture.Create<string>();
        var apprenticeship = ApprenticeshipDomainModelTestHelper.BuildApprenticeshipWithPendingStartDateChange(
            originalStartDate: new DateTime(2022, 03, 02),
            newStartDate: new DateTime(2021, 12, 14),
            originalEndDate: new DateTime(2025, 06, 23),
            newEndDate: new DateTime(2024, 03, 11));

        //Act
        apprenticeship.ApproveStartDateChange(approverUserId);

        //Assert
        apprenticeship.Episodes.Count.Should().Be(1);
        apprenticeship.LatestEpisode.EpisodePrices.Count.Should().Be(1);
        apprenticeship.LatestEpisode.LatestPrice.StartDate.Should().Be(new DateTime(2021, 12, 14));
        apprenticeship.LatestEpisode.LatestPrice.EndDate.Should().Be(new DateTime(2024, 03, 11));
    }
}