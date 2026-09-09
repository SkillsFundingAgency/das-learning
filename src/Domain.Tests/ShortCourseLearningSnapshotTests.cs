using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;
using ShortCourseLearningEntity = SFA.DAS.Learning.DataAccess.Entities.Learning.ShortCourseLearning;

namespace SFA.DAS.Learning.Domain.UnitTests
{
    [TestFixture]
    public class ShortCourseLearningSnapshotTests
    {
        [Test]
        public void FromMapsLearningLevelFields()
        {
            var learning = BuildLearning();

            var snapshot = ShortCourseLearningSnapshot.From(learning);

            snapshot.LearningKey.Should().Be(learning.Key);
            snapshot.LearnerKey.Should().Be(learning.LearnerKey);
            snapshot.TrainingCode.Should().Be(learning.TrainingCode);
            snapshot.Price.Should().Be(learning.Price);
            snapshot.LearningType.Should().Be(learning.LearningType);
        }

        [Test]
        public void FromMapsEpisodeFields()
        {
            var learning = BuildLearning();

            var snapshot = ShortCourseLearningSnapshot.From(learning);
            var episode = snapshot.Episodes.Single();
            var expectedEpisode = learning.Episodes.Single();

            episode.Key.Should().Be(expectedEpisode.Key);
            episode.Ukprn.Should().Be(expectedEpisode.Ukprn);
            episode.EmployerAccountId.Should().Be(expectedEpisode.EmployerAccountId);
            episode.LearnerRef.Should().Be(expectedEpisode.LearnerRef);
            episode.IsApproved.Should().Be(expectedEpisode.IsApproved);
            episode.IsRemoved.Should().Be(expectedEpisode.IsRemoved);
            episode.StartDate.Should().Be(expectedEpisode.StartDate);
            episode.ExpectedEndDate.Should().Be(expectedEpisode.ExpectedEndDate);
            episode.WithdrawalDate.Should().Be(expectedEpisode.WithdrawalDate);
            episode.WithdrawalReason.Should().Be(expectedEpisode.WithdrawalReason);
            episode.CompletionDate.Should().Be(expectedEpisode.CompletionDate);
            episode.ApprovalsApprenticeshipId.Should().Be(expectedEpisode.ApprovalsApprenticeshipId);
            episode.EmployerType.Should().Be(expectedEpisode.EmployerType);
            episode.TransferSenderId.Should().Be(expectedEpisode.TransferSenderId);
            episode.ForceEarningsSync.Should().Be(expectedEpisode.ForceEarningsSync);
        }

        [Test]
        public void FromMapsMilestones()
        {
            var learning = BuildLearning();

            var snapshot = ShortCourseLearningSnapshot.From(learning);

            snapshot.Episodes.Single().Milestones.Should().BeEquivalentTo(new[]
            {
                Milestone.ThirtyPercentLearningComplete,
                Milestone.LearningComplete
            });
        }

        [Test]
        public void FromMapsLearningSupport()
        {
            var learning = BuildLearning();

            var snapshot = ShortCourseLearningSnapshot.From(learning);
            var learningSupport = snapshot.Episodes.Single().LearningSupport.Single();
            var expected = learning.Episodes.Single().LearningSupport.Single();

            learningSupport.Key.Should().Be(expected.Key);
            learningSupport.StartDate.Should().Be(expected.StartDate);
            learningSupport.EndDate.Should().Be(expected.EndDate);
        }

        [Test]
        public void FromMapsMultipleEpisodes()
        {
            var learningKey = Guid.NewGuid();
            var entity = new ShortCourseLearningEntity
            {
                Key = learningKey,
                LearnerKey = Guid.NewGuid(),
                TrainingCode = "SC001",
                Price = 1000,
                LearningType = LearningType.Apprenticeship,
                Episodes = new List<ShortCourseEpisode>
                {
                    BuildEpisode(learningKey, ukprn: 11111111),
                    BuildEpisode(learningKey, ukprn: 22222222)
                }
            };
            var learning = ShortCourseLearningDomainModel.Get(entity);

            var snapshot = ShortCourseLearningSnapshot.From(learning);

            snapshot.Episodes.Should().HaveCount(2);
            snapshot.Episodes.Select(e => e.Ukprn).Should().BeEquivalentTo(new long[] { 11111111, 22222222 });
        }

        private static ShortCourseLearningDomainModel BuildLearning()
        {
            var learningKey = Guid.NewGuid();
            var entity = new ShortCourseLearningEntity
            {
                Key = learningKey,
                LearnerKey = Guid.NewGuid(),
                TrainingCode = "SC001",
                Price = 1000,
                LearningType = LearningType.Apprenticeship,
                Episodes = new List<ShortCourseEpisode> { BuildEpisode(learningKey) }
            };

            return ShortCourseLearningDomainModel.Get(entity);
        }

        private static ShortCourseEpisode BuildEpisode(Guid learningKey, long ukprn = 12345678)
        {
            var episodeKey = Guid.NewGuid();
            var episode = new ShortCourseEpisode
            {
                Key = episodeKey,
                LearningKey = learningKey,
                Ukprn = ukprn,
                EmployerAccountId = 99,
                TrainingCode = "SC001",
                LearnerRef = "LEARNER1",
                IsApproved = true,
                IsRemoved = false,
                StartDate = new DateTime(2025, 9, 1),
                ExpectedEndDate = new DateTime(2025, 12, 31),
                WithdrawalDate = new DateTime(2025, 11, 1),
                WithdrawalReason = 3,
                CompletionDate = new DateTime(2025, 12, 15),
                ApprovalsApprenticeshipId = 42,
                EmployerType = EmployerType.Levy,
                TransferSenderId = 55,
                ForceEarningsSync = true,
                Milestones = new List<ShortCourseMilestone>
                {
                    new() { Key = Guid.NewGuid(), EpisodeKey = episodeKey, Milestone = Milestone.ThirtyPercentLearningComplete },
                    new() { Key = Guid.NewGuid(), EpisodeKey = episodeKey, Milestone = Milestone.LearningComplete }
                },
                LearningSupport = new List<ShortCourseLearningSupport>
                {
                    new()
                    {
                        Key = Guid.NewGuid(),
                        LearningKey = learningKey,
                        EpisodeKey = episodeKey,
                        StartDate = new DateTime(2025, 9, 15),
                        EndDate = new DateTime(2025, 10, 15)
                    }
                }
            };

            return episode;
        }
    }
}
