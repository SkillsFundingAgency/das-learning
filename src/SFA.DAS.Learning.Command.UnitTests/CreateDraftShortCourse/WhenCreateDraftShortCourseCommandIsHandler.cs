using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.CreateDraftShortCourse;

[TestFixture]
public class WhenCreateDraftShortCourseCommandIsHandled
{
    private CreateDraftShortCourseCommandHandler _commandHandler = null!;
    private Mock<ILearnerFactory> _learnerFactory = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IShortCourseLearningFactory> _learningFactory = null!;
    private Mock<IShortCourseLearningRepository> _learningRepository = null!;
    private Mock<ILogger<CreateDraftShortCourseCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learnerFactory = new Mock<ILearnerFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningFactory = new Mock<IShortCourseLearningFactory>();
        _learningRepository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<CreateDraftShortCourseCommandHandler>>();

        _commandHandler = new CreateDraftShortCourseCommandHandler(
            _learnerFactory.Object,
            _learnerRepository.Object,
            _learningRepository.Object,
            _learningFactory.Object,
            _logger.Object);

        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenANewShortCourseLearningIsCreated()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learningEntity = _fixture.Create<ShortCourseLearning>();
        learningEntity.Episodes = new List<ShortCourseEpisode>();


        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var domainModel = ShortCourseLearningDomainModel.Get(learningEntity);

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string?>())).Returns(learnerDomainModel);
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), command.Model.OnProgramme.CompletionDate)).Returns(domainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
        result.LearningKey.Should().Be(domainModel.Key);
        domainModel.LatestEpisode.LearningType.Should().Be(command.Model.OnProgramme.LearningType);
    }

    [Test]
    public async Task ThenShortCircuitsIfApprovedEpisodeExistsWithAnotherProvider()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: command.Model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.LearningKey.Should().BeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenShortCircuitsIfApprovedEpisodeExistsWithSameProvider()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: command.Model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.LearningKey.Should().BeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenShortCircuitsIfUnapprovedEpisodeExistsWithAnotherProvider()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: command.Model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.LearningKey.Should().BeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenLearningTypeIsUpdatedWhenUpdatingExistingLearning()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: command.Model.OnProgramme.Ukprn, learningType: LearningType.Apprenticeship);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        existingLearning.LatestEpisode.LearningType.Should().Be(command.Model.OnProgramme.LearningType);
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    private ShortCourseLearningDomainModel BuildLearningWithEpisode(bool isApproved, long ukprn, LearningType learningType = LearningType.Apprenticeship)
    {
        var learningKey = Guid.NewGuid();
        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            LearnerKey = Guid.NewGuid(),
            Episodes = new List<ShortCourseEpisode>
            {
                new ShortCourseEpisode
                {
                    Key = Guid.NewGuid(),
                    LearningKey = learningKey,
                    Ukprn = ukprn,
                    EmployerAccountId = _fixture.Create<long>(),
                    TrainingCode = _fixture.Create<string>(),
                    LearnerRef = _fixture.Create<string>(),
                    IsApproved = isApproved,
                    StartDate = _fixture.Create<DateTime>(),
                    ExpectedEndDate = _fixture.Create<DateTime>(),
                    LearningType = learningType,
                    Milestones = new List<ShortCourseMilestone>(),
                    LearningSupport = new List<ShortCourseLearningSupport>()
                }
            }
        };
        return ShortCourseLearningDomainModel.Get(entity);
    }
}
