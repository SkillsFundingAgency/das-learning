using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository
{
    public class WhenGettingPrice
    {
        private LearningQueryRepository _sut;
        private Fixture _fixture;
        private LearningDataContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task ThenReturnNullWhenNoApprenticeshipFoundWithApprenticeshipKey()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();
            
            //Act
            var result = await _sut.GetPrice(_fixture.Create<Guid>());

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ThenTheCorrectPriceIsReturned()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();

            var apprenticeshipKey = _fixture.Create<Guid>();
            var episodeKey = _fixture.Create<Guid>();
            
            var episodePrice1 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episodeKey)
                .With(x => x.StartDate, new DateTime(2021, 3, 1))
                .Create();
            var episodePrice2 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episodeKey)
                .With(x => x.StartDate, new DateTime(2021, 8, 18))
                .Create();
            var episodePrice3 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episodeKey)
                .With(x => x.StartDate, new DateTime(2022, 8, 18))
                .Create();

            var episode = _fixture.Build<Episode>()
                .With(x => x.Key, episodeKey)
                .With(x => x.Prices, new List<EpisodePrice> { episodePrice1, episodePrice2, episodePrice3 })
                .Create();

            var apprenticeships = new[]
            {
                _fixture.Build<DataAccess.Entities.Learning.Learning>()
                    .With(x => x.Key, apprenticeshipKey)
                    .With(x => x.Episodes, new List<Episode>() { episode })
                    .Create(), 
            };

            await _dbContext.AddRangeAsync(apprenticeships);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetPrice(apprenticeshipKey);

            // Assert
            result.Should().NotBeNull();
            result.TotalPrice = episodePrice2.TotalPrice;
            result.AssessmentPrice = episodePrice2.EndPointAssessmentPrice;
            result.TrainingPrice = episodePrice2.TrainingPrice;
            result.FundingBandMaximum = episodePrice2.FundingBandMaximum;
            result.ApprenticeshipActualStartDate = episodePrice1.StartDate;
            result.ApprenticeshipPlannedEndDate = episodePrice2.EndDate;
            result.AccountLegalEntityId = episode.AccountLegalEntityId;
        }

        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}