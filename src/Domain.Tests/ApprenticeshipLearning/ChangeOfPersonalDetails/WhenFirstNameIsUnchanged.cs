using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenFirstNameIsUnchanged
{
    private LearnerDomainModel _learner;
    private LearningUpdateChanges[] _result;

    private string _firstName;

    [SetUp]
    public void SetUp()
    {
        (var learning, _learner) = new LearningDomainModelBuilder().Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), _learner.GetEntity());

        _firstName = updateModel.Learner.FirstName;

        //Act
        _result = _learner.Update(updateModel);
    }

    [Test]
    public void ThenPersonalDetailsAreNotMarkedAsUpdated()
    {
        _result.Should().NotContain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUnchanged()
    {
        _learner.FirstName.Should().Be(_firstName);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsNotEmitted()
    {
        _learner.FlushEvents().Should().NotContain(x => x.GetType() == typeof(PersonalDetailsChangedEvent));
    }
}