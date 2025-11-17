using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.UpdateLearner;

[TestFixture]
public class WhenUpdatingLearner
{
    private UpdateLearnerCommandHandler _commandHandler;
    private Mock<ILearningRepository> _learningRepository;
    private Mock<ILogger<UpdateLearnerCommandHandler>> _logger;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _learningRepository = new Mock<ILearningRepository>();
        _logger = new Mock<ILogger<UpdateLearnerCommandHandler>>();
        _commandHandler = new UpdateLearnerCommandHandler(_logger.Object, _learningRepository.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenTheLearnerIsUpdatedWithChanges()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var domainModel = _fixture.Create<LearningDomainModel>();

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync(domainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().NotBeEmpty();
        _learningRepository.Verify(x => x.Update(domainModel), Times.Once);

        // Note this test works because the random generated domainModel will not match the random generated command.UpdateModel and at least
        // one change will be detected.
    }

    [Test]
    public async Task ThenNoUpdateOccursIfThereAreNoChanges()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        command.UpdateModel.LearningSupport.Clear();

        var domainModel = _fixture.Create<LearningDomainModel>();

        // Create a single episode
        var singleEpisode = _fixture.Create<EpisodeDomainModel>();

        TestHelper.SetEpisode(domainModel, singleEpisode);

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync(domainModel);

        _ = domainModel.UpdateLearnerDetails(command.UpdateModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().BeEmpty();

        // the first call is to make sure the data in the domain model is up to date before the update, that way there should be no changes detected
    }

#pragma warning disable CS8620, CS8600
    [Test]
    public void ThenAnExceptionIsThrownIfTheLearnerIsNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();

        _learningRepository.Setup(x => x.Get(command.LearnerKey))
                           .ReturnsAsync((LearningDomainModel)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo($"Learning with key {command.LearnerKey} not found."));
    }
#pragma warning restore CS8620, CS8600
}
