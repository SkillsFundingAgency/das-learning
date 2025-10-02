using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using AutoFixture;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsUpdated
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string _emailAddress;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture();

        _learning = new LearningDomainModelBuilder().Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        _emailAddress = fixture.Create<string>();
        updateModel.Learning.EmailAddress = _emailAddress;

        //Act
        _result = _learning.UpdateLearnerDetails(updateModel);
    }

    [Test]
    public void ThenPersonalDetailsAreMarkedAsUpdated()
    {
        _result.Should().Contain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUpdated()
    {
        _learning.EmailAddress.Should().Be(_emailAddress);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsEmitted()
    {
        var events = _learning.FlushEvents();

        var expectedEvent = new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = _learning.ApprovalsApprenticeshipId,
            LearningKey = _learning.Key,
            FirstName = _learning.FirstName,
            LastName = _learning.LastName,
            EmailAddress = _emailAddress
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}