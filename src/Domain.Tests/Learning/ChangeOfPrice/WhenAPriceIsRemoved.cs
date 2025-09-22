using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPrice
{
    [TestFixture]
    public class WhenAPriceIsRemoved : ChangeOfPriceTestBase
    {
        private LearningDomainModel _learning;
        private LearningUpdateChanges[] _result;
        private static readonly DateTime PlannedEndDate = new(2025, 07, 31);

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
                },
                new()
                {
                    FromDate = new DateTime(2025, 03, 01),
                    TrainingPrice = 16000,
                    EpaoPrice = 2000
                }
            };

            _learning = CreateLearner(existingCosts, PlannedEndDate, 15000);

            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

            updateModel.OnProgrammeDetails.Costs.RemoveAt(1);

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);
        }

        [Test]
        public void ThenPricesAreMarkedAsUpdated()
        {
            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var prices = _learning.LatestEpisode.EpisodePrices
                .OrderBy(x => x.StartDate)
                .ToList();

            prices.Count().Should().Be(1);
            prices.Single().StartDate.Should().Be(new DateTime(2024, 08, 01));
        }

        [Test]
        public void ThenTheEndDateOfPrecedingPriceIsReset()
        {
            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var prices = _learning.LatestEpisode.EpisodePrices
                .OrderBy(x => x.StartDate)
                .ToList();

            prices.Single().EndDate.Should().Be(PlannedEndDate);
        }
    }
}
