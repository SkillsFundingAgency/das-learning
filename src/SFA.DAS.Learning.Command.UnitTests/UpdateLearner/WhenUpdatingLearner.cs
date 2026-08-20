using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SFA.DAS.Learning.Command.UnitTests.UpdateLearner;

[TestFixture]
public class WhenUpdatingLearner
{
    private UpdateLearnerCommandHandler _commandHandler;
    private Mock<ILearnerRepository> _learnerRepository;
    private Mock<IApprenticeshipLearningRepository> _learningRepository;
    private Mock<ILogger<UpdateLearnerCommandHandler>> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningRepository = new Mock<IApprenticeshipLearningRepository>();
        _logger = new Mock<ILogger<UpdateLearnerCommandHandler>>();
        _commandHandler = new UpdateLearnerCommandHandler(_logger.Object, _learnerRepository.Object, _learningRepository.Object);
        _fixture = new Fixture();
    }

    private void SetupLearningLookup(UpdateLearnerCommand command, ApprenticeshipLearningDomainModel? learningDomainModel)
    {
        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(learningDomainModel == null ? [] : [learningDomainModel]);
    }

    [Test]
    public async Task ThenTheLearnerIsUpdatedWithChanges()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        SetupLearningLookup(command, learningDomainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().NotBeEmpty();
        result.LearningKey.Should().Be(learningDomainModel.Key);
        _learnerRepository.Verify(x => x.Update(learnerDomainModel), Times.Once);
        _learningRepository.Verify(x => x.Update(learningDomainModel), Times.Once);

        // Note this test works because the random generated domainModel will not match the random generated command.UpdateModel and at least
        // one change will be detected.
    }

    [Test]
    public async Task ThenNoUpdateOccursIfThereAreNoChanges()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        command.UpdateModel.LearningSupport.Clear();
        command.UpdateModel.EnglishAndMathsCourses.Clear();

        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();

        // Create a single episode
        var singleEpisode = _fixture.Create<ApprenticeshipEpisodeDomainModel>();

        TestHelper.SetEpisode(learningDomainModel, singleEpisode);

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        SetupLearningLookup(command, learningDomainModel);

        _ = learningDomainModel.Update(command.UpdateModel);
        _ = learnerDomainModel.Update(command.UpdateModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().BeEmpty();

        // the first call is to make sure the data in the domain model is up to date before the update, that way there should be no changes detected
    }

    [Test]
    public async Task ThenARemovedApprenticeshipIsReinstatedOnUpdate()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();

        var removedEpisode = _fixture.CreateEpisodeDomainModel(x => x.IsRemoved = true);
        TestHelper.SetEpisode(learningDomainModel, removedEpisode);

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        SetupLearningLookup(command, learningDomainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().Contain(LearningUpdateChanges.Reinstated);
        learningDomainModel.LatestEpisode.IsRemoved.Should().BeFalse();
        learningDomainModel.FlushEvents().OfType<LearningReinstatedEvent>().Should().ContainSingle(e =>
            e.LearningKey == learningDomainModel.Key &&
            e.ApprenticeshipId == removedEpisode.ApprovalsApprenticeshipId);
    }

    [Test]
    public async Task ThenAnUpdateOnANonRemovedApprenticeshipDoesNotReinstate()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();

        var activeEpisode = _fixture.CreateEpisodeDomainModel(x => x.IsRemoved = false);
        TestHelper.SetEpisode(learningDomainModel, activeEpisode);

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        SetupLearningLookup(command, learningDomainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().NotContain(LearningUpdateChanges.Reinstated);
        learningDomainModel.FlushEvents().OfType<LearningReinstatedEvent>().Should().BeEmpty();
    }

    [Test]
    public void ThenAnExceptionIsThrownIfTheLearnerIsNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        SetupLearningLookup(command, null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo($"Learning for learner key {command.LearnerKey} not found."));
    }

    [Test]
    public void ThenAnExceptionIsThrownIfMoreThanOneMatchingLearningIsFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var firstLearning = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var secondLearning = _fixture.Create<ApprenticeshipLearningDomainModel>();

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn, command.TrainingCode))
            .ReturnsAsync([firstLearning, secondLearning]);

        // Act & Assert
        // Deliberately unhandled: apprenticeships can legitimately have more than one row for the
        // same (LearnerKey, Ukprn, TrainingCode) once repeats/restarts are involved. Disambiguating
        // that case is deferred to the Change of Circumstances work; for now this surfaces as an
        // unhandled InvalidOperationException rather than silently guessing.
        Assert.ThrowsAsync<InvalidOperationException>(() => _commandHandler.Handle(command));
    }
}
