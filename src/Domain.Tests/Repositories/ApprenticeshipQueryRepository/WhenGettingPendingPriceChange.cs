using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository
{
    public class WhenGettingPendingPriceChange
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
        public async Task ThenReturnNullWhenNoPriceHistoryRecordsFoundForExistingApprenticeship()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();
            var apprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), true, initiator: "Provider");

            // Act
            var result = await _sut.GetPendingPriceChange(apprenticeshipKey);

            // Assert
            result.Should().BeNull();
        }

        [TestCase("Employer")]
        [TestCase("Provider")]
        public async Task ThenTheCorrectPendingPriceChangeIsReturned(string initiator)
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();

            var apprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(apprenticeshipKey, true, initiator: initiator);
            var episodePrice = _dbContext.EpisodePrices.Single();
            var pendingRequest = _dbContext.PriceHistories.Single();
            var episode = _dbContext.Episodes.Single();
            await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), true, initiator: initiator);

            // Act
            var result = await _sut.GetPendingPriceChange(apprenticeshipKey);

            // Assert
            result.Should().NotBeNull();
            result.OriginalTrainingPrice.Should().Be(episodePrice.TrainingPrice);
            result.OriginalAssessmentPrice.Should().Be(episodePrice.EndPointAssessmentPrice);
            result.OriginalTotalPrice.Should().Be(episodePrice.TotalPrice);
            result.PendingTrainingPrice.Should().Be(10000);
            result.PendingAssessmentPrice.Should().Be(3000);
            result.PendingTotalPrice.Should().Be(13000);
            result.EffectiveFrom.Should().Be(pendingRequest.EffectiveFromDate);
            result.Reason.Should().Be("testReason");
            result.Ukprn.Should().Be(episode.Ukprn);
            result.ProviderApprovedDate.Should().Be(pendingRequest.ProviderApprovedDate);
            result.EmployerApprovedDate.Should().Be(pendingRequest.EmployerApprovedDate);
            result.AccountLegalEntityId.Should().Be(episode.AccountLegalEntityId);
            result.Initiator.Should().Be(initiator);
        }

        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}
