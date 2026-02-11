using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfExpectedEndDate;

[TestFixture]
public class WhenExpectedEndDateIsUpdated
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {

        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithGeneratedCosts(1)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();

        //Act
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
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

        prices.Count().Should().Be(1);
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