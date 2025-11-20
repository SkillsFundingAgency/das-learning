using AutoFixture;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Domain.UnitTests.Helpers
{
    public class LearningDomainModelBuilder
    {
        private readonly IFixture _fixture = new Fixture();
        private List<Cost> _costs;
        private int _numberOfCosts = 1;
        private DateTime _plannedEndDate;
        private int _fundingBandMaximum;

        public LearningDomainModelBuilder WithCosts(List<Cost> costs)
        {
            _costs = costs;
            return this;
        }

        public LearningDomainModelBuilder WithGeneratedCosts(int count)
        {
            _numberOfCosts = count;
            return this;
        }

        public LearningDomainModelBuilder WithPlannedEndDate(DateTime date)
        {
            _plannedEndDate = date;
            return this;
        }

        public LearningDomainModelBuilder WithFundingBandMaximum(int maximum)
        {
            _fundingBandMaximum = maximum;
            return this;
        }

        public LearningDomainModel Build()
        {
            var costs = _costs ?? _fixture.CreateMany<Cost>(_numberOfCosts).ToList();
            var orderedCosts = costs.OrderBy(c => c.FromDate).ToList();

            var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
            entity.CompletionDate = entity.CompletionDate?.Date;
            entity.MathsAndEnglishCourses.Clear();

            var episode = _fixture.Create<DataAccess.Entities.Learning.Episode>();
            episode.LearningKey = entity.Key;
            episode.FundingBandMaximum = _fundingBandMaximum;
            episode.Prices.Clear();
            episode.LearningSupport.Clear();

            for (int i = 0; i < orderedCosts.Count; i++)
            {
                var cost = orderedCosts[i];
                var isLast = i == orderedCosts.Count - 1;
                var endDate = isLast ? _plannedEndDate : orderedCosts[i + 1].FromDate.AddDays(-1);

                episode.Prices.Add(new DataAccess.Entities.Learning.EpisodePrice
                {
                    Key = Guid.NewGuid(),
                    StartDate = cost.FromDate,
                    EndDate = endDate,
                    TrainingPrice = cost.TrainingPrice,
                    EndPointAssessmentPrice = cost.EpaoPrice,
                    TotalPrice = cost.TotalPrice
                });
            }

            entity.Episodes = [episode];
            return LearningDomainModel.Get(entity);
        }
    }
}
