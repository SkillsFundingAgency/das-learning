using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Models.UpdateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.CreateDraftApprenticeshipLearning;

[TestFixture]
public class WhenCreateDraftApprenticeshipLearningCommandIsHandled
{
    private const long _Ukprn = 12345678;
    private Fixture _fixture = new Fixture();
    private Mock<ILearnerRepository> _learnerRepository;
    private Mock<IApprenticeshipLearningRepository> _learningRepository;
    private Mock<ILogger<CreateDraftApprenticeshipLearningCommandHandler>> _logger;

    private CreateDraftApprenticeshipLearningCommandHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningRepository = new Mock<IApprenticeshipLearningRepository>();
        _logger = new Mock<ILogger<CreateDraftApprenticeshipLearningCommandHandler>>();

        _handler = new CreateDraftApprenticeshipLearningCommandHandler(
            _learnerRepository.Object,
            _learningRepository.Object,
            _logger.Object);
    }

    // This test can be deleted after additional functionality is added to the handler
    [Test]
    public async Task Then_Null_Is_Returned_When_Learner_Does_Not_Exist()
    {
        // Arrange
        var command = CreateCommand();

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .Returns(Task.FromResult<LearnerDomainModel?>(null));

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }

    // This test can be deleted after additional functionality is added to the handler
    [Test]
    public async Task Then_Null_Is_Returned_When_Learning_Does_Not_Exist()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .Returns(Task.FromResult<LearnerDomainModel?>(learner));

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .Returns(Task.FromResult<ApprenticeshipLearningDomainModel?>(null));

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }

    // This test can be deleted after additional functionality is added to the handler
    [Test]
    public async Task Then_Null_Is_Returned_When_Latest_Episode_Is_Not_Removed()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Active);


        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Then_Learning_And_Learner_Are_Updated_When_Reinstating()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        // Act
        await _handler.Handle(command);

        // Assert
        _learnerRepository.Verify(x => x.Update(learner), Times.Once);
        _learningRepository.Verify(x => x.Update(learning), Times.Once);
    }

    [Test]
    public async Task Then_Result_Is_Returned_When_Reinstating_Learning()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.LearningKey.Should().Be(learning.Key);
        result.LearningEpisodeKey.Should().Be(learning.LatestEpisode.Key);
    }

    [Test]
    public async Task Then_PersonalDetailsChangedEvent_Is_Added_When_Personal_Details_Have_Changed()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        // Act
        await _handler.Handle(command);

        // Assert
        AssertPersonalDetailsEvent(learner, learning.LatestEpisode.ApprovalsApprenticeshipId, learning.Key, learner.FirstName, learner.LastName);
    }

    private void AssertPersonalDetailsEvent(
        LearnerDomainModel domainModel,
        long approvalsApprenticeshipId,
        Guid learningKey,
        string firstName,
        string lastName)
    {
        var domainEvent = domainModel.FlushEvents().OfType<PersonalDetailsChangedEvent>().SingleOrDefault();

        domainEvent.Should().NotBeNull();

        domainEvent!.ApprovalsApprenticeshipId.Should().Be(approvalsApprenticeshipId);
        domainEvent.LearningKey.Should().Be(learningKey);
        domainEvent.FirstName.Should().Be(firstName);
        domainEvent.LastName.Should().Be(lastName);

    }

    private CreateDraftApprenticeshipLearningCommand CreateCommand()
    {
        var command = new CreateDraftApprenticeshipLearningCommand(_Ukprn, _fixture.Create<LearningUpdateContext>());
        return command;
    }

    private LearnerDomainModel CreateLearner()
    {
        var entity = _fixture.Create<Learner>();
        return LearnerDomainModel.Get(entity);
    }

    private enum EpisodeStatus
    {
        Active,
        Removed
    }

    private ApprenticeshipLearningDomainModel CreateLearning(EpisodeStatus episodeStatus)
    {
        var entity = _fixture.Create<ApprenticeshipLearning>();
        var episode = _fixture.Create<ApprenticeshipEpisode>();
        episode.Ukprn = _Ukprn;

        if (episodeStatus == EpisodeStatus.Active)
        {
            episode.IsRemoved = false;
        }
        else 
        { 
            episode.IsRemoved = true;
        }

        entity.Episodes = new List<ApprenticeshipEpisode> { episode };

        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}