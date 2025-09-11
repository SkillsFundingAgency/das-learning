using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenUpdatingLearningSupport
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenNoChangesAreMadeWhenNoLearningSupportExistsAndNoneProvided()
    {
        //Arrange
        var learningSupport = new List<LearningSupportDetails>();
        var learning = CreateLearner(learningSupport);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().BeEmpty();
        learning.LatestEpisode.LearningSupport.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportIsAddedWhenNoLearningExistsAndSomeProvided()
    {
        //Arrange
        var learningSupport = new List<LearningSupportDetails>();
        var learning = CreateLearner(learningSupport);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());

        updateModel.LearningSupport.Add(new LearningSupportDetails { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30)});

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.LearningSupport);
        learning.LatestEpisode.LearningSupport.Should().HaveCount(1);
        learning.LatestEpisode.LearningSupport.Should().Contain(x =>
            x.StartDate == updateModel.LearningSupport[0].StartDate &&
            x.EndDate == updateModel.LearningSupport[0].EndDate);

    }

    [Test]
    public void ThenLearningSupportIsRemovedWhenLearningExistsAndNoneProvided()
    {
        //Arrange
        var learningSupport = new List<LearningSupportDetails>
        {
            new LearningSupportDetails { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30)}
        };
        var learning = CreateLearner(learningSupport);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.LearningSupport.Clear();

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.LearningSupport);
        learning.LatestEpisode.LearningSupport.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportIsUpdatedWhenLearningExistsAndSomeProvided()
    {
        //Arrange
        var learningSupport = new List<LearningSupportDetails>
        {
            new LearningSupportDetails{ StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) }
        };
        var learning = CreateLearner(learningSupport);
        var updateModel = LearnerUpdateModelHelper.CreateFromLearningEntity(learning.GetEntity());
        updateModel.LearningSupport.Add(new LearningSupportDetails { StartDate = DateTime.Now.AddDays(31), EndDate = DateTime.Now.AddDays(60) });

        //Act
        var result = learning.UpdateLearnerDetails(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.LearningSupport);
    }


    private LearningDomainModel CreateLearner(List<LearningSupportDetails> learningSupport)
    {
        var entity = _fixture.Create<DataAccess.Entities.Learning.Learning>();
        entity.CompletionDate = entity.CompletionDate?.Date;

        var episode = _fixture.Create<DataAccess.Entities.Learning.Episode>();
        episode.LearningKey = entity.Key;

        episode.LearningSupport = learningSupport.ConvertAll(x => new DataAccess.Entities.Learning.LearningSupport
        {
            Key = _fixture.Create<Guid>(),
            LearningKey = entity.Key,
            EpisodeKey = episode.Key,
            StartDate = x.StartDate,
            EndDate = x.EndDate
        });
        entity.Episodes = new List<DataAccess.Entities.Learning.Episode> { episode };

        return LearningDomainModel.Get(entity);
    }

}