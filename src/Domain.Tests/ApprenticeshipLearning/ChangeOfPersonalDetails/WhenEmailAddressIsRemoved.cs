using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsRemoved
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {
        (_learning, _learner) = new LearningDomainModelBuilder().Build();
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

        updateModel.Learner.EmailAddress = null;

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
        _learner.EmailAddress.Should().BeNull();
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
            LastName = _learner.LastName,
            EmailAddress = null
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}