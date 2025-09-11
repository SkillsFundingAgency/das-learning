using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning
{
    [TestFixture]
    public class WhenUpdatingLearningPrices
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void ThenPricesAreNotUpdatedIfChanged()
        {
            //Arrange
            var existingCosts = new List<Cost>
            {
                new()
                {
                    FromDate = new DateTime(2024, 08, 01),
                    TrainingPrice = 10000,
                    EpaoPrice = 1000
                }
            };

            var learning = CreateLearner(existingCosts, new DateTime(2025, 07, 31), 15000);
            
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());

            updateModel.OnProgrammeDetails.Costs.Add(new Cost
            {
                FromDate = new DateTime(2024,12,01),
                TrainingPrice = 12000,
                EpaoPrice = 1000
            });

            //Act
            var result = learning.UpdateLearnerDetails(updateModel);

            //Assert
            result.Should().Contain(LearningUpdateChanges.Prices);
        }

        [Test]
        public void ThenPricesAreNotUpdatedIfNotChanged()
        {
            //Arrange
            var existingCosts = new List<Cost>
            {
                new()
                {
                    FromDate = new DateTime(2024, 08, 01),
                    TrainingPrice = 10000,
                    EpaoPrice = 1000
                }
            };

            var learning = CreateLearner(existingCosts, new DateTime(2025,07,31), 15000);
            var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());

            //Act
            var result = learning.UpdateLearnerDetails(updateModel);

            //Assert
            result.Should().BeEmpty();
        }

        private LearningDomainModel CreateLearner(List<Cost> costs, DateTime plannedEndDate, int fundingBandMaximum)
        {
            var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
            entity.CompletionDate = entity.CompletionDate?.Date;

            var episode = _fixture.Create<Episode>();
            episode.LearningKey = entity.Key;
            episode.Prices.Clear();
            episode.LearningSupport.Clear();
            entity.MathsAndEnglishCourses.Clear();

            var orderedCosts = costs.OrderBy(c => c.FromDate).ToList();

            for (var i = 0; i < orderedCosts.Count; i++)
            {
                var cost = orderedCosts[i];
                var isLast = i == orderedCosts.Count - 1;
                var endDate = isLast
                    ? plannedEndDate
                    : orderedCosts[i + 1].FromDate.AddDays(-1);

                episode.Prices.Add(new EpisodePrice
                {
                    StartDate = cost.FromDate,
                    EndDate = endDate,
                    TrainingPrice = cost.TrainingPrice,
                    EndPointAssessmentPrice = cost.EpaoPrice,
                    TotalPrice = cost.TotalPrice,
                    FundingBandMaximum = fundingBandMaximum,
                    IsDeleted = false
                });
            }

            entity.Episodes = [episode];
            return LearningDomainModel.Get(entity);
        }


    }
}
