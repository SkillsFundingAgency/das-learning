using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

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
        (var learning, var learner) = CreateLearner(learningSupport);
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), learner.GetEntity());

        //Act
        var result = learning.Update(updateModel);

        //Assert
        result.Should().BeEmpty();
        learning.LatestEpisode.LearningSupport.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportIsAddedWhenNoLearningExistsAndSomeProvided()
    {
        //Arrange
        var learningSupport = new List<LearningSupportDetails>();
        (var learning, var learner) = CreateLearner(learningSupport);
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), learner.GetEntity());

        updateModel.LearningSupport.Add(new LearningSupportDetails { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30)});

        //Act
        var result = learning.Update(updateModel);

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
        (var learning, var learner) = CreateLearner(learningSupport);
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), learner.GetEntity());
        updateModel.LearningSupport.Clear();

        //Act
        var result = learning.Update(updateModel);

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
        (var learning, var learner) = CreateLearner(learningSupport);
        var updateModel = LearningUpdateModelHelper.CreateUpdateModel(learning.GetEntity(), learner.GetEntity());
        updateModel.LearningSupport.Add(new LearningSupportDetails { StartDate = DateTime.Now.AddDays(31), EndDate = DateTime.Now.AddDays(60) });

        //Act
        var result = learning.Update(updateModel);

        //Assert
        result.Should().Contain(x => x == LearningUpdateChanges.LearningSupport);
    }


    private (ApprenticeshipLearningDomainModel, LearnerDomainModel) CreateLearner(List<LearningSupportDetails> learningSupport)
    {
        var learningEntity = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        learningEntity.CompletionDate = learningEntity.CompletionDate?.Date;

        var episode = _fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        episode.LearningKey = learningEntity.Key;
        episode.PauseDate = null;

        episode.LearningSupport = learningSupport.ConvertAll(x => new DataAccess.Entities.Learning.LearningSupport
        {
            Key = _fixture.Create<Guid>(),
            LearningKey = learningEntity.Key,
            EpisodeKey = episode.Key,
            StartDate = x.StartDate,
            EndDate = x.EndDate
        });

        episode.Prices.Clear();
        episode.Prices.Add(_fixture.Build<EpisodePrice>()
            .Create());

        learningEntity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };

        var learnerEntity = _fixture.Create<DataAccess.Entities.Learning.Learner>();
        learnerEntity.Key = learningEntity.LearnerKey;

        var learner = LearnerDomainModel.Get(learnerEntity);
        var learning = ApprenticeshipLearningDomainModel.Get(learningEntity);

        return new(learning, learner);
    }

}