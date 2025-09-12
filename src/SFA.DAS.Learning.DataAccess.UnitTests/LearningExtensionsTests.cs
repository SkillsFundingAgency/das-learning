using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.DataAccess.Extensions;

namespace SFA.DAS.Learning.DataAccess.UnitTests
{
    [TestFixture]
    public class LearningExtensionsTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void GetEpisode_ShouldReturnLatestActiveEpisode_WhenActiveEpisodeExists()
        {
            // Arrange
            var activeEpisode = CreateEpisode(startDate: DateTime.Today.AddMonths(-1));
            var apprenticeship = CreateApprenticeshipWithEpisodes(activeEpisode);

            // Act
            var result = apprenticeship.GetEpisode();

            // Assert
            result.Should().Be(activeEpisode);
        }

        [Test]
        public void GetEpisode_ShouldReturnLatestEpisode_WhenNoActiveEpisodeExists()
        {
            // Arrange
            var latestEpisode = CreateEpisode(startDate: DateTime.Today.AddMonths(-2));
            var apprenticeship = CreateApprenticeshipWithEpisodes(latestEpisode);

            // Act
            var result = apprenticeship.GetEpisode();

            // Assert
            result.Should().Be(latestEpisode);
        }

        [Test]
        public void GetEpisode_ShouldReturnActiveEpisodeAmongMultiple_WhenOneIsActive()
        {
            // Arrange
            var deletedEpisode = CreateEpisode(startDate: DateTime.Today.AddMonths(-2));
            var activeEpisode = CreateEpisode(startDate: DateTime.Today.AddMonths(-1));
            var apprenticeship = CreateApprenticeshipWithEpisodes(deletedEpisode, activeEpisode);

            // Act
            var result = apprenticeship.GetEpisode();

            // Assert
            result.Should().Be(activeEpisode);
        }

        [Test]
        public void GetEpisode_ShouldThrowInvalidOperationException_WhenNoEpisodesExist()
        {
            // Arrange
            var apprenticeship = CreateApprenticeshipWithEpisodes();

            // Act
            Action action = () => apprenticeship.GetEpisode();

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("No active episode found");
        }

        [TestCase("2000-05-10", "2020-05-01", 19)]  // Birthday hasn't happened yet in the year
        [TestCase("2000-05-10", "2020-05-10", 20)]  // Birthday on the start date
        [TestCase("2000-05-10", "2020-06-01", 20)]  // Birthday has already happened
        public void GetAgeAtStartOfApprenticeship_ShouldReturnCorrectAge(string dob, string startDate, int expectedAge)
        {
            // Arrange
            var apprenticeship = _fixture.Build<Entities.Learning.Learning>()
                .With(a => a.DateOfBirth, DateTime.Parse(dob))
                .Create();
            apprenticeship.Episodes.Add(CreateEpisode(startDate: DateTime.Parse(startDate)));

            // Act
            var result = apprenticeship.GetAgeAtStartOfApprenticeship();

            // Assert
            result.Should().Be(expectedAge);
        }

        [Test]
        public void GetStartDate_ShouldReturnEarliestStartDate_WithNonDeletedPrices()
        {
            // Arrange
            var episode1 = CreateEpisode(startDate: new DateTime(2021, 1, 1));
            var episode2 = CreateEpisode(startDate: new DateTime(2020, 1, 1));
            var apprenticeship = CreateApprenticeshipWithEpisodes(episode1, episode2);

            // Act
            var result = apprenticeship.GetStartDate();

            // Assert
            result.Should().Be(new DateTime(2020, 1, 1));
        }

        [Test]
        public void GetStartDate_ShouldIgnoreDeletedPrices()
        {
            // Arrange
            var episodeWithDeletedPrice = CreateEpisode(startDate: new DateTime(2021, 1, 1));
            var episodeWithActivePrice = CreateEpisode(startDate: new DateTime(2020, 1, 1));
            var apprenticeship = CreateApprenticeshipWithEpisodes(episodeWithDeletedPrice, episodeWithActivePrice);

            // Act
            var result = apprenticeship.GetStartDate();

            // Assert
            result.Should().Be(new DateTime(2020, 1, 1));
        }

        [Test]
        public void GetPlannedEndDate_ShouldReturnLatestEndDate()
        {
            // Arrange
            var episode1 = CreateEpisode(endDate: new DateTime(2022, 12, 1));
            var episode2 = CreateEpisode(endDate: new DateTime(2023, 5, 1));
            var apprenticeship = CreateApprenticeshipWithEpisodes(episode1, episode2);

            // Act
            var result = apprenticeship.GetPlannedEndDate();

            // Assert
            result.Should().Be(new DateTime(2023, 5, 1));
        }

        private Entities.Learning.Learning CreateApprenticeshipWithEpisodes(params Episode[] episodes)
        {
            var apprenticeship = _fixture.Create<Entities.Learning.Learning>();
            apprenticeship.Episodes = new List<Episode>();
            apprenticeship.Episodes.AddRange(episodes);
            return apprenticeship;
        }

        private Episode CreateEpisode(DateTime? startDate = null, DateTime? endDate = null)
        {
            return _fixture.Build<Episode>()
                .With(e => e.Prices, new List<EpisodePrice>
                {
                    new EpisodePrice
                    {
                        StartDate = startDate ?? DateTime.Today,
                        EndDate = endDate ?? DateTime.Today.AddYears(1)
                    }
                })
                .Create();
        }
    }
}
