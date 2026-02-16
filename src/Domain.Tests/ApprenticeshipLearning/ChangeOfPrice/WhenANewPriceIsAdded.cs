using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning.ChangeOfPrice;

[TestFixture]
public class WhenANewPriceIsAdded
{
    private LearnerDomainModel _learner;
    private ApprenticeshipLearningDomainModel _learning;
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

        (_learning, _learner) = new LearningDomainModelBuilder()
            .WithCosts(existingCosts)
            .WithPlannedEndDate(new DateTime(2025, 07, 31))
            .Build();

        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(_learning.GetEntity(), _learner.GetEntity());

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