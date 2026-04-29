using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsUpdated
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string _emailAddress;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture();

        (_learning, _learner) = new LearningDomainModelBuilder().Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        _emailAddress = fixture.Create<string>();
        updateModel.Learner.EmailAddress = _emailAddress;

        //Act
        _result = _learner.Update(updateModel);
    }

    [Test]
    public void ThenPersonalDetailsAreMarkedAsUpdated()
    {
        _result.Should().Contain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUpdated()
    {
        _learner.EmailAddress.Should().Be(_emailAddress);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsEmitted()
    {
        var events = _learner.FlushEvents();

        var expectedEvent = new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = _learning.LatestEpisode.ApprovalsApprenticeshipId,
            LearningKey = _learning.Key,
            FirstName = _learner.FirstName,
            LastName = _learner.LastName,
            EmailAddress = _emailAddress
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}