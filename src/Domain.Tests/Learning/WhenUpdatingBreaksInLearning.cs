using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenUpdatingBreaksInLearning
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenNoChangesAreMadeWhenNoBreaksExistAndNoneProvided()
    {
        // Arrange
        var breaks = new List<EpisodeBreakInLearning>();
        var learning = CreateLearner(breaks);

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.OnProgrammeDetails.BreaksInLearning.Clear();

        // Act
        var result = learning.UpdateLearnerDetails(updateModel);

        // Assert
        result.Should().BeEmpty();
        learning.LatestEpisode.EpisodeBreaksInLearning.Should().BeEmpty();
    }

    [Test]
    public void ThenBreaksAreAddedWhenNoneExistAndSomeProvided()
    {
        // Arrange
        var breaks = new List<EpisodeBreakInLearning>();
        var learning = CreateLearner(breaks);

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.OnProgrammeDetails.BreaksInLearning.Add(new BreakInLearningUpdateDetails
        {
            StartDate = DateTime.Now.Date,
            EndDate = DateTime.Now.AddDays(10).Date
        });

        // Act
        var result = learning.UpdateLearnerDetails(updateModel);

        // Assert
        result.Should().Contain(LearningUpdateChanges.BreaksInLearningUpdated);
        learning.LatestEpisode.EpisodeBreaksInLearning.Should().HaveCount(1);

        learning.LatestEpisode.EpisodeBreaksInLearning.Should().Contain(x =>
            x.StartDate == updateModel.OnProgrammeDetails.BreaksInLearning[0].StartDate &&
            x.EndDate == updateModel.OnProgrammeDetails.BreaksInLearning[0].EndDate);
    }

    [Test]
    public void ThenBreaksAreRemovedWhenExistingBreaksExistAndNoneProvided()
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

        var learning = CreateLearner(breaks);

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.OnProgrammeDetails.BreaksInLearning.Clear();

        // Act
        var result = learning.UpdateLearnerDetails(updateModel);

        // Assert
        result.Should().Contain(LearningUpdateChanges.BreaksInLearningUpdated);
        learning.LatestEpisode.EpisodeBreaksInLearning.Should().BeEmpty();
    }

    [Test]
    public void ThenBreaksAreUpdatedWhenExistingBreaksExistAndSomeProvided()
    {
        // Arrange
        var breaks = new List<EpisodeBreakInLearning>
        {
            new EpisodeBreakInLearning
            {
                Key = Guid.NewGuid(),
                EpisodeKey = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(-20).Date,
                EndDate = DateTime.Now.AddDays(-10).Date
            }
        };

        var learning = CreateLearner(breaks);

        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.OnProgrammeDetails.BreaksInLearning.Add(new BreakInLearningUpdateDetails
        {
            StartDate = DateTime.Now.Date,
            EndDate = DateTime.Now.AddDays(5).Date
        });

        // Act
        var result = learning.UpdateLearnerDetails(updateModel);

        // Assert
        result.Should().Contain(LearningUpdateChanges.BreaksInLearningUpdated);
    }

    private LearningDomainModel CreateLearner(List<EpisodeBreakInLearning> breaks)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.CompletionDate = entity.CompletionDate?.Date;

        var episode = _fixture.Create<DataAccess.Entities.Learning.Episode>();
        episode.LearningKey = entity.Key;
        episode.PauseDate = null;

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

        entity.Episodes = new List<DataAccess.Entities.Learning.Episode> { episode };

        return LearningDomainModel.Get(entity);
    }
}