using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfPrice
{
    [TestFixture]
    public class WhenAPriceIsUnchanged : ChangeOfPriceTestBase
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

            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(_learning.GetEntity());

            //Act
            _result = _learning.UpdateLearnerDetails(updateModel);
        }

        [Test]
        public void ThenPricesAreNotMarkedAsUpdated()
        {


            //Assert
            var prices = _learning.LatestEpisode.EpisodePrices
                .OrderBy(x => x.StartDate)
                .ToList();

            prices.Count.Should().Be(1);
            prices.Single().Key.Should().NotBe(Guid.Empty);
            _result.Should().BeEmpty();
        }

    }
}
