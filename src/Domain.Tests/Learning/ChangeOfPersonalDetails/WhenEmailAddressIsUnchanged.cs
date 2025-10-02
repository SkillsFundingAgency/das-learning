using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPersonalDetails;

[TestFixture]
public class WhenEmailAddressIsUnchanged
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    private string? _emailAddress;

    [SetUp]
    public void SetUp()
    {
        _learning = new LearningDomainModelBuilder().Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        _emailAddress = updateModel.Learning.EmailAddress;

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
        _learning.EmailAddress.Should().Be(_emailAddress);
    }

    [Test]
    public void ThenAPersonalDetailsEventIsNotEmitted()
    {
        _learning.FlushEvents().Should().NotContain(x => x.GetType() == typeof(PersonalDetailsChangedEvent));
    }
}