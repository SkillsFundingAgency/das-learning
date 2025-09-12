using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPrice
{
    [TestFixture]
    public class WhenAPriceIsUpdated : ChangeOfPriceTestBase
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

            _learning = CreateLearner(existingCosts, new DateTime(2025, 07, 31), 15000);
        }

        [Test]
        public void AndStartDateIsChangedThenPricesAreUpdated()
        {
            //Arrange
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
            updateModel.OnProgrammeDetails.Costs.Single().FromDate = new DateTime(2024, 06, 01);

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);

            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var prices = _learning.LatestEpisode.EpisodePrices
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StartDate)
                .ToList();

            prices.Count().Should().Be(1);
            prices.Single().StartDate.Should().Be(new DateTime(2024, 06, 01));
            prices.Single().Key.Should().Be(Guid.Empty);
        }

        [Test]
        public void AndTrainingPriceIsChangedThenPricesAreUpdated()
        {
            //Arrange
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
            updateModel.OnProgrammeDetails.Costs.Single().TrainingPrice += 1000;

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);

            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var price = _learning.LatestEpisode.EpisodePrices
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StartDate)
                .Single();

            price.TrainingPrice.Should().Be(11000);
            price.Key.Should().NotBe(Guid.Empty);
        }

        [Test]
        public void AndEpaoPriceIsChangedThenPricesAreUpdated()
        {
            //Arrange
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
            updateModel.OnProgrammeDetails.Costs.Single().EpaoPrice += 200;

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);

            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var price = _learning.LatestEpisode.EpisodePrices
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StartDate)
                .Single();

            price.EndPointAssessmentPrice.Should().Be(1200);
            price.Key.Should().NotBe(Guid.Empty);
        }

        [Test]
        public void AndEpaoPriceIsChangedToUnknownThenPricesAreUpdated()
        {
            //Arrange
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());
            updateModel.OnProgrammeDetails.Costs.Single().EpaoPrice = null;

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);

            //Assert
            _result.Should().Contain(LearningUpdateChanges.Prices);

            var price = _learning.LatestEpisode.EpisodePrices
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StartDate)
                .Single();

            price.EndPointAssessmentPrice.Should().Be(null);
            price.TotalPrice.Should().Be(10000);
            price.Key.Should().NotBe(Guid.Empty);
        }
    }
}
