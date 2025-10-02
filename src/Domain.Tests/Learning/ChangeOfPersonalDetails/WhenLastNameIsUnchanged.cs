using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenLastNameIsUnchanged
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string _lastName;

    [SetUp]
    public void SetUp()
    {
        _learning = new LearningDomainModelBuilder().Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        _lastName = updateModel.Learning.LastName;

        //Act
        _result = _learning.UpdateLearnerDetails(updateModel);
    }

    [Test]
    public void ThenPersonalDetailsAreNotMarkedAsUpdated()
    {
        _result.Should().NotContain(LearningUpdateChanges.PersonalDetails);
    }

    [Test]
    public void DomainModelIsUnchanged()
    {
        _learning.LastName.Should().Be(_lastName);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsNotEmitted()
    {
        _learning.FlushEvents().Should().NotContain(x => x.GetType() == typeof(PersonalDetailsChangedEvent));
    }
}