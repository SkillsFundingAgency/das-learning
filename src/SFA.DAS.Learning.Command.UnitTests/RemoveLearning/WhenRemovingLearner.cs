using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.UnitTests.RemoveLearning;


[TestFixture]
public class WhenRemovingLearner
{
    private RemoveLearnerCommandHandler _commandHandler;
    private Mock<ILearningRepository> _learningRepository;
    private Mock<IMessageSession> _messageSession;
    private Mock<ILogger<RemoveLearnerCommandHandler>> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _learningRepository = new Mock<ILearningRepository>();
        _messageSession = new Mock<IMessageSession>();
        _logger = new Mock<ILogger<RemoveLearnerCommandHandler>>();

        _commandHandler = new RemoveLearnerCommandHandler(
            _learningRepository.Object,
            _messageSession.Object,
            _logger.Object);

        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenTheLearnerIsRemovedAndRepositoryUpdated()
    {
        // Arrange
        var command = _fixture.Create<RemoveLearnerCommand.RemoveLearnerCommand>();
        var domainModel = _fixture.Create<LearningDomainModel>();

        var latestEpisode = _fixture.CreateEpisodeDomainModel(x => x.FundingPlatform = FundingPlatform.SLD);

        TestHelper.SetEpisode(domainModel, latestEpisode);

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync(domainModel);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Update(domainModel), Times.Once);
        _messageSession.Verify(x => x.Publish(It.IsAny<LearningWithdrawnEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void ThenAnExceptionIsThrownIfTheLearnerIsNotFound()
    {
        // Arrange
        var command = _fixture.Create<RemoveLearnerCommand.RemoveLearnerCommand>();
        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync((LearningDomainModel)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex.Message, Is.EqualTo($"Learning with key {command.LearnerKey} not found."));
    }

    [Test]
    public async Task ThenAnEventIsPublishedIfFundingPlatformIsDas()
    {
        // Arrange
        var command = _fixture.Create<RemoveLearnerCommand.RemoveLearnerCommand>();
        var domainModel = _fixture.Create<LearningDomainModel>();

        var latestEpisode = _fixture.CreateEpisodeDomainModel(x =>
        {
            x.FundingPlatform = FundingPlatform.DAS;
            x.LastDayOfLearning = DateTime.Today;
        });

        TestHelper.SetEpisode(domainModel, latestEpisode);

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync(domainModel);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(
            It.Is<LearningWithdrawnEvent>(e =>
                e.LearningKey == domainModel.Key &&
                e.ApprovalsApprenticeshipId == domainModel.ApprovalsApprenticeshipId &&
                e.Reason == WithdrawReason.WithdrawFromStart.ToString() &&
                e.LastDayOfLearning == latestEpisode.LastDayOfLearning &&
                e.EmployerAccountId == latestEpisode.EmployerAccountId), 
            It.IsAny<PublishOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenTheLastDayOfLearningIsReturned()
    {
        // Arrange
        var command = _fixture.Create<RemoveLearnerCommand.RemoveLearnerCommand>();
        var domainModel = _fixture.Create<LearningDomainModel>();

        var latestEpisode = _fixture.CreateEpisodeDomainModel(x => x.FundingPlatform = FundingPlatform.SLD);

        TestHelper.SetEpisode(domainModel, latestEpisode);

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync(domainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.LastDayOfLearning.Should().Be(latestEpisode.LastDayOfLearning);
    }
}
