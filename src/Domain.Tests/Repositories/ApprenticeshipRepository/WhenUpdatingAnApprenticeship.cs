using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipRepository
{
    public class WhenUpdatingAnApprenticeship
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
        public async Task ThenApprenticeshipUpdatedInDataStore()
        {
            // Arrange
            await SetUpApprenticeshipRepository();
            var apprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(apprenticeshipKey, false);
            var apprenticeship = await _dbContext.Apprenticeships
                .Include(x => x.Episodes)
                .SingleAsync(x => x.Key == apprenticeshipKey);
            var domainModel = LearningDomainModel.Get(apprenticeship);
            var newDateOfBirth = _fixture.Create<DateTime>();
            domainModel.GetEntity().DateOfBirth = newDateOfBirth;

            // Act
            await _sut.Update(domainModel);
            
            // Assert
            _dbContext.ApprenticeshipsDbSet.Count().Should().Be(1);
            _dbContext.Apprenticeships.Single().DateOfBirth.Should().Be(newDateOfBirth);
        }

        [Test]
        public async Task ThenEpisodeUpdatedInDataStore()
        {
            // Arrange
            await SetUpApprenticeshipRepository();
            var apprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(apprenticeshipKey, false);
            var apprenticeship = await _dbContext.Apprenticeships
                .Include(x => x.Episodes)
                .SingleAsync(x => x.Key == apprenticeshipKey);
            var domainModel = LearningDomainModel.Get(apprenticeship);
            domainModel.LatestEpisode.GetEntity().LegalEntityName = "alternative_name";

            // Act
            await _sut.Update(domainModel);
            
            // Assert
            _dbContext.Apprenticeships.Count().Should().Be(1);
            _dbContext.Episodes.Single().LegalEntityName.Should().Be("alternative_name");
        }

        [Test]
        public async Task ThenEpisodePriceUpdatedInDataStore()
        {
            // Arrange
            await SetUpApprenticeshipRepository();
            var apprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(apprenticeshipKey, false);
            var apprenticeship = await _dbContext.Apprenticeships
                .Include(x => x.Episodes)
                .ThenInclude(y => y.Prices)
                .SingleAsync(x => x.Key == apprenticeshipKey);
            var domainModel = LearningDomainModel.Get(apprenticeship);
            domainModel.LatestEpisode.LatestPrice.GetEntity().FundingBandMaximum = 12345;

            // Act
            await _sut.Update(domainModel);
            
            // Assert
            _dbContext.Apprenticeships.Count().Should().Be(1);
            _dbContext.Episodes.Single().Prices.Single().FundingBandMaximum.Should().Be(12345);
        }

        private async Task SetUpApprenticeshipRepository()
        {
            _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            _apprenticeshipFactory = new Mock<ILearningFactory>();
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            _sut = new LearningRepository(new Lazy<LearningDataContext>(_dbContext),
                _domainEventDispatcher.Object, _apprenticeshipFactory.Object);
        }
    }
}
