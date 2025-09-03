using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipRepository
{
    public class WhenGettingAnApprenticeship
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
        public async Task ThenApprenticeshipIsRetrieved()
        {
            // Arrange
            SetUpApprenticeshipRepository();
            var expectedApprenticeship = LearningDomainModel.Get(_fixture.Create<DataAccess.Entities.Learning.Learning>());
            _apprenticeshipFactory
                .Setup(x => x.GetExisting(It.IsAny<DataAccess.Entities.Learning.Learning>())).Returns(expectedApprenticeship);

            // Act
            await _sut.Add(expectedApprenticeship);
            var actualApprenticeship = await _sut.Get(expectedApprenticeship.Key);

            // Assert
            actualApprenticeship.Should().BeEquivalentTo(expectedApprenticeship);
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
