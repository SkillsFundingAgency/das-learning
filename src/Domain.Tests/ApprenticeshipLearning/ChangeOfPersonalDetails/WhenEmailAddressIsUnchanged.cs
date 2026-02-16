using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsUnchanged
{
    private LearnerDomainModel _learner;
    private LearningUpdateChanges[] _result;

    private string? _emailAddress;

    [SetUp]
    public void SetUp()
    {
        (var learning, _learner) = new LearningDomainModelBuilder().Build();
        var eventBuilder = new LearnerUpdatedEventBuilder(_learner, learning);
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), _learner.GetEntity());

        _emailAddress = updateModel.Learner.EmailAddress;

        //Act
        _result = _learner.Update(updateModel, eventBuilder);
    }

    [Test]
    public void ThenPersonalDetailsAreNotMarkedAsUpdated()
    {
        _result.Should().NotContain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUnchanged()
    {
        _learner.EmailAddress.Should().Be(_emailAddress);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsNotEmitted()
    {
        _learner.FlushEvents().Should().NotContain(x => x.GetType() == typeof(PersonalDetailsChangedEvent));
    }
}