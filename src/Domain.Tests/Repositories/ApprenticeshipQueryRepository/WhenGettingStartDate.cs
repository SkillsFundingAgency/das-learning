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
    public class WhenGettingStartDate
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
            // Arrange
            var apprenticeshipKey = _fixture.Create<Guid>();
            SetUpApprenticeshipQueryRepository();

            //Act
            var result = await _sut.GetStartDate(_fixture.Create<Guid>());

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ThenTheCorrectStartDateIsReturned()
        {
            // Arrange
            var apprenticeshipKey = _fixture.Create<Guid>();
            SetUpApprenticeshipQueryRepository();

            await _dbContext.AddApprenticeship(apprenticeshipKey, false);
            var apprenticeship = _dbContext.Apprenticeships.Single(x => x.Key == apprenticeshipKey);
            var episode = _dbContext.Episodes.Single(x => x.LearningKey == apprenticeshipKey);
            var episodePrice = _dbContext.EpisodePrices.Single(x => x.EpisodeKey == episode.Key);
            await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false);
            await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false);

            // Act
            var result = await _sut.GetStartDate(apprenticeshipKey);

            // Assert
            result.Should().NotBeNull();
            result.ActualStartDate.Should().Be(episodePrice.StartDate);
            result.PlannedEndDate.Should().Be(episodePrice.EndDate);
            result.AccountLegalEntityId.Should().Be(episode.AccountLegalEntityId);
            result.ApprenticeDateOfBirth.Should().Be(apprenticeship.DateOfBirth);
            result.CourseCode.Should().Be(episode.TrainingCode);
            result.CourseVersion.Should().Be(episode.TrainingCourseVersion);
            result.SimplifiedPaymentsMinimumStartDate.Should().Be(new DateTime(2024, 11, 1));
        }

        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}
