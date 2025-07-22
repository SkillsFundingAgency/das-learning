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
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository
{
    public class WhenGettingPriceHistory
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
            var result = await _sut.GetPendingPriceChange(_fixture.Create<Guid>());

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ThenTheCorrectPendingPriceChangeIsReturned()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();
            
            var apprenticeshipKey = _fixture.Create<Guid>();
            var episode1Key = _fixture.Create<Guid>();
            var episode2Key = _fixture.Create<Guid>();
            var episodePrice1 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episode1Key)
                .With(x => x.StartDate, new DateTime(2021, 3, 1))
                .With(x => x.IsDeleted, false)
                .Create();
            var episodePrice2 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episode2Key)
                .With(x => x.StartDate, new DateTime(2021, 8, 18))
                .With(x => x.IsDeleted, false)
                .Create();
            var episodePrice3 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episode2Key)
                .With(x => x.StartDate, new DateTime(2022, 8, 18))
                .With(x => x.IsDeleted, true)
                .Create();
            var episodePrice4 = _fixture.Build<EpisodePrice>()
                .With(x => x.EpisodeKey, episode2Key)
                .With(x => x.StartDate, new DateTime(2023, 8, 18))
                .With(x => x.IsDeleted, false)
                .Create();
            var priceHistoryKey = _fixture.Create<Guid>();
            var effectiveFromDate = DateTime.UtcNow.AddDays(-5).Date;
            var episode1 = _fixture.Build<Episode>()
                .With(x => x.Key, episode1Key)
                .With(x => x.Prices, new List<EpisodePrice> { episodePrice1})
                .Create();
            var episode2 = _fixture.Build<Episode>()
                .With(x => x.Key, episode2Key)
                .With(x => x.Prices, new List<EpisodePrice> { episodePrice2, episodePrice3, episodePrice4 })
                .Create();
            var apprenticeships = new[]
            {
                _fixture.Build<DataAccess.Entities.Learning.Learning>()
                    .With(x => x.Key, apprenticeshipKey)
                    .With(x => x.PriceHistories, new List<PriceHistory>() { new()
                        {
                            Key = priceHistoryKey,
                            LearningKey = apprenticeshipKey,
                            PriceChangeRequestStatus = ChangeRequestStatus.Created,
                            TrainingPrice = 10000,
                            AssessmentPrice = 3000,
                            TotalPrice = 13000,
                            EffectiveFromDate = effectiveFromDate,
                            ChangeReason = "testReason"
                        }
                    })
                    .With(x => x.Episodes, new List<Episode>() { episode1, episode2 })
                    .Create(), 
            };

            await _dbContext.AddRangeAsync(apprenticeships);
            await _dbContext.SaveChangesAsync();
            
            // Act
            var result = await _sut.GetPendingPriceChange(apprenticeshipKey);

            // Assert
            result.Should().NotBeNull();
            result.OriginalTrainingPrice = episodePrice4.TrainingPrice;
            result.OriginalAssessmentPrice = episodePrice4.EndPointAssessmentPrice;
            result.OriginalTotalPrice = episodePrice4.TotalPrice;
            result.PendingTrainingPrice = 10000;
            result.PendingAssessmentPrice = 3000;
            result.PendingTotalPrice = 13000;
            result.EffectiveFrom = effectiveFromDate;
            result.Reason = "testReason";
            result.Ukprn = episode2.Ukprn;
        }
        
        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}
