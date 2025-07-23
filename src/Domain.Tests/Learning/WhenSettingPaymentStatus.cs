using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

public class WhenSettingPaymentStatus
{
    private Fixture _fixture;
    private const string _userId = "AnyUserId";
    private LearningFactory _learningFactory;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public void And_NewStatusIsSameAsOld_Then_Throws()
    {
        //Arrange
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        var userId = _fixture.Create<string>();

        //Act / Assert
        var action = () => apprenticeship.SetPaymentsFrozen(false, userId, DateTime.Now);

        //Assert
        action.Should().Throw<InvalidOperationException>().WithMessage($"Payments are already unfrozen for this apprenticeship: {apprenticeship.Key}.");
    }

    [Test]
    public void And_NewStatusIsFrozen_Then_PaymentsAreFrozen()
    {
        //Arrange
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        var userId = _fixture.Create<string>();
        var timeChanged = DateTime.Now;

        //Act 
        apprenticeship.SetPaymentsFrozen(true, userId, timeChanged);

        //Assert
        apprenticeship.LatestEpisode.PaymentsFrozen.Should().Be(true);
        apprenticeship.FreezeRequests.Count.Should().Be(1);
        apprenticeship.FreezeRequests.Count( x=> 
            x.FrozenBy == userId && 
            x.FrozenDateTime == timeChanged && 
            !x.Unfrozen).Should().Be(1);
    }

    [Test]
    public void And_NewStatusIsUnfreeze_Then_PaymentsAreActive()
    {
        //Arrange
        var userIdFreeze = _fixture.Create<string>();
        var userIdUnfreeze = _fixture.Create<string>();
        var timefreeze = DateTime.Now.AddMinutes(-10);
        var timeUnfreeze = DateTime.Now;
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        apprenticeship.SetPaymentsFrozen(true, userIdFreeze, timefreeze);

        //Act 
        apprenticeship.SetPaymentsFrozen(false, userIdUnfreeze, timeUnfreeze);

        //Assert
        apprenticeship.LatestEpisode.PaymentsFrozen.Should().Be(false);
        apprenticeship.FreezeRequests.Count(x => 
            x.FrozenBy == userIdFreeze && 
            x.FrozenDateTime == timefreeze &&
            x.UnfrozenBy == userIdUnfreeze &&
            x.UnfrozenDateTime == timeUnfreeze &&
            x.Unfrozen).Should().Be(1);
    }
}
