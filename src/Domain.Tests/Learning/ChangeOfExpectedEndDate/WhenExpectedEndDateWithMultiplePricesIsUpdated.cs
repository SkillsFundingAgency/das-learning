using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Linq;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfExpectedEndDate;

[TestFixture]
public class WhenExpectedEndDateWithMultiplePricesIsUpdated
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {
        _learning = new LearningDomainModelBuilder()
            .WithGeneratedCosts(3)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .WithFundingBandMaximum(15000)
            .Build();

        //Act
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2026, 07, 31);

        _result = _learning.UpdateLearnerDetails(updateModel);
    }

    [Test]
    public void ThenExpectedEndDateIsMarkedAsUpdated()
    {
        _result.Should().Contain(LearningUpdateChanges.ExpectedEndDate);
        _result.Should().NotContain(LearningUpdateChanges.Prices);

        var prices = _learning.LatestEpisode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .ToList();

        prices.Count().Should().Be(3);
        prices.Last().EndDate.Should().Be(new DateTime(2026, 07, 31));
    }

    [Test]
    public void ThenAnEndDateChangedEventIsEmitted()
    {
        var events = _learning.FlushEvents();

        var expectedEvent = new EndDateChangedEvent
        {
            ApprovalsApprenticeshipId = _learning.ApprovalsApprenticeshipId,
            LearningKey = _learning.Key,
            PlannedEndDate = new DateTime(2026, 07, 31) //todo: can we make this non-nullable?
        };

        events.Should().ContainEquivalentOf(expectedEvent);
    }
}