using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipRepository
{
    public class WhenAddingAnApprenticeship
    {
        private LearningRepository _sut;
        private Fixture _fixture;
        private LearningDataContext _dbContext;
        private Mock<IDomainEventDispatcher> _domainEventDispatcher;
        private Mock<ILearningFactory> _apprenticeshipFactory;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize(new ApprenticeshipCustomization());
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task ThenApprenticeshipAddedToDataStore()
        {
            // Arrange
            var apprenticeship = _fixture.Create<LearningDomainModel>();
            SetUpApprenticeshipRepository();

            // Act
            await _sut.Add(apprenticeship);
            
            // Assert
            _dbContext.ApprenticeshipsDbSet.Count().Should().Be(1);

            var storedApprenticeship = _dbContext.ApprenticeshipsDbSet.Single();
            var expectedModel = apprenticeship.GetEntity();

            expectedModel.Should().BeEquivalentTo(storedApprenticeship);
        }

        [Test]
        public async Task ThenEpisodeAddedToDataStore()
        {
            // Arrange
            var apprenticeship = _fixture.Create<LearningDomainModel>();
            SetUpApprenticeshipRepository();
            var episodePrice = _fixture.Build<EpisodePrice>().Create();
            var episode = EpisodeDomainModel.Get(_fixture.Build<Episode>()
                .With(x => x.Prices, new List<EpisodePrice>(){ episodePrice })
                .With(x => x.PaymentsFrozen, false)
                .With(x => x.LearningStatus, "Active")
                .With(x => x.LastDayOfLearning, (DateTime?)null)
                .Create());

            apprenticeship.AddEpisode(
                episode.Ukprn,
                episode.EmployerAccountId,
                episodePrice.StartDate,
                episodePrice.EndDate,
                episodePrice.TotalPrice,
                episodePrice.TrainingPrice,
                episodePrice.EndPointAssessmentPrice,
                episode.FundingType,
                episode.FundingPlatform,
                episodePrice.FundingBandMaximum,
                episode.FundingEmployerAccountId,
                episode.LegalEntityName,
                episode.AccountLegalEntityId,
                episode.TrainingCode,
                episode.TrainingCourseVersion);

            // Act
            await _sut.Add(apprenticeship);
            
            // Assert
            _dbContext.Episodes.Count().Should().Be(1);
            var storedEpisode = _dbContext.Episodes.Single();
            storedEpisode.Should().BeEquivalentTo(episode, x => x
                .Excluding(y => y.Key)
                .Excluding(y => y.LatestPrice)
                .Excluding(y => y.EpisodePrices)
                .Excluding(y => y.FirstPrice)
                .Excluding(y => y.ActiveEpisodePrices)
                .Excluding(y => y.LearningStatus)
                .Excluding(y => y.IsWithdrawnBackToStart)
                .Excluding(y => y.LearningSupport));
        }

        private void SetUpApprenticeshipRepository()
        {
            _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            _apprenticeshipFactory = new Mock<ILearningFactory>();
            
            _dbContext =
                InMemoryDbContextCreator.SetUpInMemoryDbContext();
            _sut = new LearningRepository(new Lazy<LearningDataContext>(_dbContext),
                _domainEventDispatcher.Object, _apprenticeshipFactory.Object);
        }
    }
}
