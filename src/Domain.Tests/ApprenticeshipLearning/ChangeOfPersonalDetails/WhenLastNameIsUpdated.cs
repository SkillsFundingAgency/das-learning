using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenLastNameIsUpdated
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string _lastName;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture();

        (_learning, _learner) = new LearningDomainModelBuilder().Build();
        var eventBuilder = new LearnerUpdatedEventBuilder(_learner, _learning);

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        _lastName = fixture.Create<string>();
        updateModel.Learner.LastName = _lastName;

        //Act
        _result = _learner.Update(updateModel, eventBuilder);
    }

    [Test]
    public void ThenPersonalDetailsAreMarkedAsUpdated()
    {
        _result.Should().Contain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUpdated()
    {
        _learner.LastName.Should().Be(_lastName);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsEmitted()
    {
        var events = _learner.FlushEvents();

        var expectedEvent = new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = _learning.ApprovalsApprenticeshipId,
            LearningKey = _learning.Key,
            FirstName = _learner.FirstName,
            LastName = _lastName,
            EmailAddress = _learner.EmailAddress ?? ""
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}