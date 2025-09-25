using AutoFixture;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning.ChangeOfExpectedEndDate
{
    public class ChangeOfExpectedEndDateTestBase
    {
        protected Fixture Fixture;

        protected ChangeOfExpectedEndDateTestBase()
        {
            Fixture = new Fixture();
        }

        protected LearningDomainModel CreateLearner(int numberOfPriceEpisodes, DateTime plannedEndDate, int fundingBandMaximum)
        {
            var costs = Fixture.CreateMany<Cost>(numberOfPriceEpisodes);

            var entity = Fixture.Create<DataAccess.Entities.Learning.Learning>();
            entity.CompletionDate = entity.CompletionDate?.Date;

            var episode = Fixture.Create<DataAccess.Entities.Learning.Episode>();
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

                episode.Prices.Add(new DataAccess.Entities.Learning.EpisodePrice
                {
                    Key = Guid.NewGuid(),
                    StartDate = cost.FromDate,
                    EndDate = endDate,
                    TrainingPrice = cost.TrainingPrice,
                    EndPointAssessmentPrice = cost.EpaoPrice,
                    TotalPrice = cost.TotalPrice,
                    FundingBandMaximum = fundingBandMaximum
                });
            }

            entity.Episodes = [episode];
            return LearningDomainModel.Get(entity);
        }

    }
}
