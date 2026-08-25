using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.UnitTests.RemoveLearning;


[TestFixture]
public class WhenRemovingLearner
{
    private const int AcademicYear = 2526; // 2025-08-01 to 2026-07-31
    private static readonly DateTime InAcademicYearStartDate = new(2025, 9, 1);
    private static readonly DateTime InAcademicYearEndDate = new(2026, 6, 30);
    private static readonly DateTime OutsideAcademicYearStartDate = new(2026, 9, 1);
    private static readonly DateTime OutsideAcademicYearEndDate = new(2027, 6, 30);

    private RemoveLearnerCommandHandler _commandHandler;
    private Mock<IApprenticeshipLearningRepository> _learningRepository;
    private Mock<ILogger<RemoveLearnerCommandHandler>> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _learningRepository = new Mock<IApprenticeshipLearningRepository>();
        _logger = new Mock<ILogger<RemoveLearnerCommandHandler>>();

        _commandHandler = new RemoveLearnerCommandHandler(
            _learningRepository.Object,
            _logger.Object);

        _fixture = new Fixture();
    }

    private RemoveLearnerCommand.RemoveLearnerCommand CreateCommand()
    {
        var command = _fixture.Create<RemoveLearnerCommand.RemoveLearnerCommand>();
        command.AcademicYear = AcademicYear;
        return command;
    }

    private ApprenticeshipLearningDomainModel CreateLearningInAcademicYear(Action<ApprenticeshipEpisode>? configureEpisode = null)
    {
        var domainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();
        TestHelper.SetCompletionDate(domainModel, null);

        var episode = _fixture.CreateEpisodeDomainModel(x =>
        {
            x.WithdrawalDate = null;
            x.IsRemoved = false;
            x.Prices = [new DataAccess.Entities.Learning.EpisodePrice { Key = Guid.NewGuid(), StartDate = InAcademicYearStartDate, EndDate = InAcademicYearEndDate, TotalPrice = 1000 }];
            configureEpisode?.Invoke(x);
        });

        TestHelper.SetEpisode(domainModel, episode);
        return domainModel;
    }

    private ApprenticeshipLearningDomainModel CreateLearningOutsideAcademicYear(Action<ApprenticeshipEpisode>? configureEpisode = null)
    {
        var domainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();
        TestHelper.SetCompletionDate(domainModel, null);

        var episode = _fixture.CreateEpisodeDomainModel(x =>
        {
            x.WithdrawalDate = null;
            x.IsRemoved = false;
            x.Prices = [new DataAccess.Entities.Learning.EpisodePrice { Key = Guid.NewGuid(), StartDate = OutsideAcademicYearStartDate, EndDate = OutsideAcademicYearEndDate, TotalPrice = 1000 }];
            configureEpisode?.Invoke(x);
        });

        TestHelper.SetEpisode(domainModel, episode);
        return domainModel;
    }

    [Test]
    public async Task ThenTheLearnerIsRemovedAndRepositoryUpdated()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear();

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
                   .ReturnsAsync([domainModel]);

        ApprenticeshipLearningDomainModel? updatedModel = null;

        _learningRepository
            .Setup(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(m => updatedModel = m);

        // Act
        var removedLearningKeys = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        updatedModel.Should().NotBeNull();
        updatedModel!.LatestEpisode.IsRemoved.Should().BeTrue();
        updatedModel.FlushEvents().OfType<LearningRemovedEvent>().Should().ContainSingle();
        removedLearningKeys.Should().ContainSingle().Which.Should().Be(domainModel.Key);
    }

    [Test]
    public async Task ThenEnglishAndMathsIsRemoved()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear(x => x.FundingPlatform = FundingPlatform.SLD);

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([domainModel]);

        ApprenticeshipLearningDomainModel? updatedModel = null;

        _learningRepository
            .Setup(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(m => updatedModel = m);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);

        updatedModel.Should().NotBeNull();
        updatedModel!.EnglishAndMathsCourses.Should().BeEmpty();
    }

    [Test]
    public async Task ThenLearningSupportIsRemoved()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear(x => x.FundingPlatform = FundingPlatform.SLD);

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([domainModel]);

        ApprenticeshipLearningDomainModel? updatedModel = null;

        _learningRepository
            .Setup(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(m => updatedModel = m);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);

        updatedModel.Should().NotBeNull();
        updatedModel!.LatestEpisode.LearningSupport.Should().BeEmpty();
    }

    [Test]
    public async Task ThenBreaksInLearningAreRemoved()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear(x => x.FundingPlatform = FundingPlatform.SLD);

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([domainModel]);

        ApprenticeshipLearningDomainModel? updatedModel = null;

        _learningRepository
            .Setup(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(m => updatedModel = m);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);

        updatedModel.Should().NotBeNull();
        updatedModel!.LatestEpisode.EpisodeBreaksInLearning.Should().BeEmpty();
    }

    [Test]
    public void ThenAnExceptionIsThrownIfTheLearnerIsNotFound()
    {
        // Arrange
        var command = CreateCommand();
        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
                           .ReturnsAsync([]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex.Message, Is.EqualTo($"Learning for learner key {command.LearnerKey} in {command.AcademicYear} AY not found."));
    }

    [Test]
    public void ThenAnExceptionIsThrownIfNoLearningsOverlapTheGivenAcademicYear()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningOutsideAcademicYear();

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
                           .ReturnsAsync([domainModel]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex.Message, Is.EqualTo($"Learning for learner key {command.LearnerKey} in {command.AcademicYear} AY not found."));
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenOnlyLearningsOverlappingTheGivenAcademicYearAreRemoved()
    {
        // Arrange
        var command = CreateCommand();
        var inScopeLearning = CreateLearningInAcademicYear();
        var outOfScopeLearning = CreateLearningOutsideAcademicYear();

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([inScopeLearning, outOfScopeLearning]);

        // Act
        var removedLearningKeys = await _commandHandler.Handle(command);

        // Assert
        removedLearningKeys.Should().ContainSingle().Which.Should().Be(inScopeLearning.Key);
        inScopeLearning.LatestEpisode.IsRemoved.Should().BeTrue();
        outOfScopeLearning.LatestEpisode.IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(inScopeLearning), Times.Once);
        _learningRepository.Verify(x => x.Update(outOfScopeLearning), Times.Never);
    }

    [Test]
    public async Task ThenADomainEventIsRaisedWithCorrectProperties()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear();

        _learningRepository.Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
                   .ReturnsAsync([domainModel]);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        var domainEvent = domainModel.FlushEvents().OfType<LearningRemovedEvent>().Single();
        domainEvent.LearningKey.Should().Be(domainModel.Key);
        domainEvent.ApprenticeshipId.Should().Be(domainModel.LatestEpisode.ApprovalsApprenticeshipId);
    }

    [Test]
    public async Task ThenAllLearningsForTheLearnerAreRemovedAndReturned()
    {
        // Arrange
        var command = CreateCommand();
        var firstLearning = CreateLearningInAcademicYear();
        var secondLearning = CreateLearningInAcademicYear();

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([firstLearning, secondLearning]);

        // Act
        var removedLearningKeys = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Exactly(2));
        removedLearningKeys.Should().BeEquivalentTo([firstLearning.Key, secondLearning.Key]);
        firstLearning.LatestEpisode.IsRemoved.Should().BeTrue();
        secondLearning.LatestEpisode.IsRemoved.Should().BeTrue();
    }

    [Test]
    public async Task ThenRemovalIsScopedToTheGivenUkprn()
    {
        // Arrange
        var command = CreateCommand();
        var domainModel = CreateLearningInAcademicYear();

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn))
            .ReturnsAsync([domainModel]);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.GetAllByLearnerKey(command.LearnerKey, command.Ukprn), Times.Once);
    }
}
