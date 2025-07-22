using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenAStartDateChangeIsCancelled
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenTheStartDateChangeRecordIsUpdated()
    {
        //Arrange
        var rejectReason = _fixture.Create<string>();
        var apprenticeship = ApprenticeshipDomainModelTestHelper.BuildApprenticeshipWithPendingStartDateChange();

        //Act
        apprenticeship.CancelPendingStartDateChange();

        //Assert
        var entity = apprenticeship.GetEntity();
        entity.StartDateChanges.Any(x => x.RequestStatus == ChangeRequestStatus.Created).Should().BeFalse();
        entity.StartDateChanges.Should().Contain(x => x.RequestStatus == ChangeRequestStatus.Cancelled);
    }
}