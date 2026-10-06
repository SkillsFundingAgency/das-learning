using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using LearningRemovedEvent = SFA.DAS.Learning.Domain.Events.LearningRemovedEvent;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenRemovingApprenticeship
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenIsRemovedIsSetOnTheEpisode()
    {
        // Arrange
        (var learning, _) = CreateLearner(new List<ApprenticeshipLearningSupport>(), new List<EpisodeBreakInLearning>());

        // Act
        learning.RemoveLearner();

        // Assert
        learning.LatestEpisode.IsRemoved.Should().BeTrue();
    }

    [Test]
    public void AndTheEpisodeIsApproved_ThenALearningRemovedEventIsRaised()
    {
        // Arrange
        (var learning, _) = CreateLearner(new List<ApprenticeshipLearningSupport>(), new List<EpisodeBreakInLearning>(), isApproved: true);

        // Act
        learning.RemoveLearner();

        // Assert
        var removedEvent = learning.FlushEvents().OfType<LearningRemovedEvent>().Should().ContainSingle().Subject;
        removedEvent.LearningKey.Should().Be(learning.Key);
        removedEvent.ApprenticeshipId.Should().Be(learning.LatestEpisode.ApprovalsApprenticeshipId);
    }

    [Test]
    public void AndTheEpisodeIsAnUnapprovedDraft_ThenItIsRemovedButNoLearningRemovedEventIsRaised()
    {
        // Arrange - Approvals has no record of a draft (ApprovalsApprenticeshipId is 0), so must not be told about its removal
        (var learning, _) = CreateLearner(new List<ApprenticeshipLearningSupport>(), new List<EpisodeBreakInLearning>(), isApproved: false);

        // Act
        learning.RemoveLearner();

        // Assert
        learning.LatestEpisode.IsRemoved.Should().BeTrue();
        learning.FlushEvents().OfType<LearningRemovedEvent>().Should().BeEmpty();
    }

    [Test]
    public void ThenWithdrawalDateIsNotSet()
    {
        // Arrange
        (var learning, _) = CreateLearner(new List<ApprenticeshipLearningSupport>(), new List<EpisodeBreakInLearning>());

        // Act
        learning.RemoveLearner();

        // Assert
        learning.LatestEpisode.WithdrawalDate.Should().BeNull();
    }

    [Test]
    public void ThenLearningSupportIsCleared()
    {
        // Arrange
        var learningSupport = new List<ApprenticeshipLearningSupport>
        {
            new ApprenticeshipLearningSupport
            {
                Key = Guid.NewGuid(),
                EpisodeKey = Guid.NewGuid(),
                LearningKey = Guid.NewGuid(),
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddMonths(1).Date
            }
        };

        (var learning, _) = CreateLearner(learningSupport, new List<EpisodeBreakInLearning>());

        // Act
        learning.RemoveLearner();

        // Assert
        learning.LatestEpisode.LearningSupport.Should().BeEmpty();
    }

    [Test]
    public void ThenBreaksInLearningAreCleared()
    {
        // Arrange
        var breaks = new List<EpisodeBreakInLearning>
        {
            new EpisodeBreakInLearning
            {
                Key = Guid.NewGuid(),
                EpisodeKey = Guid.NewGuid(),
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.AddDays(15).Date
            }
        };

        (var learning, _) = CreateLearner(new List<ApprenticeshipLearningSupport>(), breaks);

        // Act
        learning.RemoveLearner();

        // Assert
        learning.LatestEpisode.EpisodeBreaksInLearning.Should().BeEmpty();
    }

    [Test]
    public void ThenEnglishAndMathsIsCleared()
    {
        // Arrange
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();

        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        episode.LearningKey = entity.Key;
        episode.PauseDate = null;

        episode.LearningSupport = new List<ApprenticeshipLearningSupport>();
        episode.BreaksInLearning = new List<EpisodeBreakInLearning>();
        episode.Prices.Clear();
        episode.Prices.Add(_fixture.Build<EpisodePrice>().Create());

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };

        entity.EnglishAndMathsCourses = new List<EnglishAndMaths>
        {
            new EnglishAndMaths
            {
                Key = Guid.NewGuid(),
                LearningKey = entity.Key,
                Course = "maths",
                LearnAimRef = "ZMTH0001"
            }
        };

        var learning = ApprenticeshipLearningDomainModel.Get(entity);

        // Act
        learning.RemoveLearner();

        // Assert
        learning.EnglishAndMathsCourses.Should().BeEmpty();
    }

    private (ApprenticeshipLearningDomainModel, LearnerDomainModel) CreateLearner(
        List<ApprenticeshipLearningSupport> learningSupport,
        List<EpisodeBreakInLearning> breaks,
        bool isApproved = true)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();

        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        episode.LearningKey = entity.Key;
        episode.PauseDate = null;
        episode.WithdrawalDate = null;
        episode.IsApproved = isApproved;

        episode.LearningSupport = new List<ApprenticeshipLearningSupport>();
        foreach (var support in learningSupport)
        {
            episode.LearningSupport.Add(new ApprenticeshipLearningSupport
            {
                Key = _fixture.Create<Guid>(),
                EpisodeKey = episode.Key,
                LearningKey = entity.Key,
                StartDate = support.StartDate,
                EndDate = support.EndDate
            });
        }

        episode.BreaksInLearning = new List<EpisodeBreakInLearning>();
        foreach (var b in breaks)
        {
            episode.BreaksInLearning.Add(new EpisodeBreakInLearning
            {
                Key = _fixture.Create<Guid>(),
                EpisodeKey = episode.Key,
                StartDate = b.StartDate,
                EndDate = b.EndDate
            });
        }

        // Minimal valid episode price
        episode.Prices.Clear();
        episode.Prices.Add(_fixture.Build<EpisodePrice>().Create());

        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };

        var learnerEntity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        learnerEntity.Key = entity.LearnerKey;

        var learner = LearnerDomainModel.Get(learnerEntity);
        var learning = ApprenticeshipLearningDomainModel.Get(entity);
        return (learning, learner);
    }
}
