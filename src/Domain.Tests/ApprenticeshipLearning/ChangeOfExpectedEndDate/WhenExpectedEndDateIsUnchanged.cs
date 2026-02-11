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
public class WhenExpectedEndDateIsUnchanged
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {
        //_learning = CreateLearner(1, new DateTime(2025, 07, 31), 15000);

        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithGeneratedCosts(1)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();


        //Act
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());
        updateModel.OnProgrammeDetails.ExpectedEndDate = new DateTime(2025, 07, 31);

        _result = _learning.UpdateLearnerDetails(updateModel);
    }

    [Test]
    public void ThenExpectedEndDateIsNotMarkedAsUpdated()
    {
        _result.Should().NotContain(LearningUpdateChanges.ExpectedEndDate);

        var prices = _learning.LatestEpisode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .ToList();

        prices.Count().Should().Be(1);
        prices.Last().EndDate.Should().Be(new DateTime(2025, 07, 31));
    }

    [Test]
    public void ThenAnEndDateChangedEventIsNotEmitted()
    {
        var events = _learning.FlushEvents();
        events.Should().NotContain(x => x.GetType() == typeof(EndDateChangedEvent));
    }
}