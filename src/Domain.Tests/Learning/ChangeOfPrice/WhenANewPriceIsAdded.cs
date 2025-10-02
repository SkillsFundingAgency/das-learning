using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPrice;

[TestFixture]
public class WhenANewPriceIsAdded
{
    private LearningDomainModel _learning;
    private LearningUpdateChanges[] _result;

    [SetUp]
    public void SetUp()
    {
        var existingCosts = new List<Cost>
        {
            new()
            {
                FromDate = new DateTime(2024, 08, 01),
                TrainingPrice = 10000,
                EpaoPrice = 1000
            }
        };

        _learning = new LearningDomainModelBuilder()
            .WithCosts(existingCosts)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .WithFundingBandMaximum(15000)
            .Build();

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

        updateModel.OnProgrammeDetails.Costs.Add(new Cost
        {
            FromDate = new DateTime(2024, 12, 01),
            TrainingPrice = 12000,
            EpaoPrice = 1000
        });

        //Act
        _result = _learning.UpdateLearnerDetails(updateModel);
    }

    [Test]
    public void ThenPricesAreMarkedAsUpdated()
    {
        _result.Should().Contain(LearningUpdateChanges.Prices);

        var prices = _learning.LatestEpisode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .ToList();

        prices.Count().Should().Be(2);
        prices.Last().Key.Should().Be(Guid.Empty);
    }

    [Test]
    public void ThenPrecedingPriceEndDateIsSet()
    {
        var prices = _learning.LatestEpisode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .ToList();

        prices.First().EndDate.Should().Be(new DateTime(2024, 11, 30));
    }
}