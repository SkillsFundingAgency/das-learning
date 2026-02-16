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
using SFA.DAS.Learning.Domain.Builders;

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

    [Test]
    public async Task ThenTheLearnerIsUpdatedWithChanges()
    {
        // Arrange
        var command = _fixture.Create<UpdateLearnerCommand>();
        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        _learningRepository.Setup(x => x.Get(command.LearningKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Changes.Should().NotBeEmpty();
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
        command.UpdateModel.MathsAndEnglishCourses.Clear();

        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var learningDomainModel = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var eventBuilder = new LearnerUpdatedEventBuilder(learnerDomainModel, learningDomainModel);

        // Create a single episode
        var singleEpisode = _fixture.Create<ApprenticeshipEpisodeDomainModel>();

        TestHelper.SetEpisode(learningDomainModel, singleEpisode);

        _learnerRepository.Setup(x => x.Get(learningDomainModel.LearnerKey))
            .ReturnsAsync(learnerDomainModel);
        _learningRepository.Setup(x => x.Get(command.LearningKey))
            .ReturnsAsync(learningDomainModel);

        _ = learningDomainModel.UpdateLearnerDetails(command.UpdateModel, eventBuilder);
        _ = learnerDomainModel.Update(command.UpdateModel);

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

        _learningRepository.Setup(x => x.Get(command.LearningKey))
                           .ReturnsAsync((ApprenticeshipLearningDomainModel)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _commandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo($"Learning with key {command.LearningKey} not found."));
    }
#pragma warning restore CS8620, CS8600
}
