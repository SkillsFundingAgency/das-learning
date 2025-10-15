using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenFirstNameIsUpdated
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string _firstName;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture();

        _learning = new LearningDomainModelBuilder().Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        _firstName = fixture.Create<string>();
        updateModel.Learning.FirstName = _firstName;

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
        _learning.FirstName.Should().Be(_firstName);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsEmitted()
    {
        var events = _learning.FlushEvents();

        var expectedEvent = new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = _learning.ApprovalsApprenticeshipId,
            LearningKey = _learning.Key,
            FirstName = _firstName,
            LastName = _learning.LastName,
            EmailAddress = _learning.EmailAddress ?? ""
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}