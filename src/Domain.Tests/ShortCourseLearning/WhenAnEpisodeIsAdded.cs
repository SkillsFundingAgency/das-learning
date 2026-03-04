using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using System;
using System.Linq;
using FluentAssertions;

namespace SFA.DAS.Learning.Domain.UnitTests.ShortCourseLearning
{
    [TestFixture]
    public class WhenAnEpisodeIsAdded
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void ThenAnEpisodeAndMilestonesAreAdded()
        {
            // Arrange
            var learnerKey = Guid.NewGuid();
            var shortCourse = ShortCourseLearningDomainModel.New(learnerKey, null);

            var ukprn = _fixture.Create<long>();
            var employerAccountId = _fixture.Create<long>();
            var trainingCode = _fixture.Create<string>();
            var milestones = _fixture.CreateMany<Milestone>().ToList();

            // Act
            shortCourse.AddEpisode(
                ukprn,
                employerAccountId,
                trainingCode,
                false,
                DateTime.UtcNow.AddMonths(-2),
                DateTime.UtcNow.AddMonths(6),
                null,
                milestones);

            // Assert
            var episode = shortCourse.Episodes.Single();
            episode.Ukprn.Should().Be(ukprn);
            episode.EmployerAccountId.Should().Be(employerAccountId);
            episode.TrainingCode.Should().Be(trainingCode);

            episode.Milestones.Should().HaveCount(milestones.Count);
            episode.Milestones.Select(m => m.Milestone)
                .Should().BeEquivalentTo(milestones);
        }
    }
}
