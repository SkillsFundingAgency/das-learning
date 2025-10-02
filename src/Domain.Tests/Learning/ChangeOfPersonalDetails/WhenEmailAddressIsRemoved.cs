using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsRemoved
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {
        _learning = new LearningDomainModelBuilder().Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        updateModel.Learning.EmailAddress = null;

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
        _learning.EmailAddress.Should().BeNull();
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
            EmailAddress = null
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}