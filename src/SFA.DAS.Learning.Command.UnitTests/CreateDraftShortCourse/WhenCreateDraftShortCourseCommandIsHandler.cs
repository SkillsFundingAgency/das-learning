using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Models.Dtos;
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
    private Mock<IShortCourseLearningDomainModelMapper> _mapper = null!;
    private Mock<ILogger<CreateDraftShortCourseCommandHandler>> _logger = null!;
    private FeatureFlags _featureFlags = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learnerFactory = new Mock<ILearnerFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningFactory = new Mock<IShortCourseLearningFactory>();
        _learningRepository = new Mock<IShortCourseLearningRepository>();
        _mapper = new Mock<IShortCourseLearningDomainModelMapper>();
        _logger = new Mock<ILogger<CreateDraftShortCourseCommandHandler>>();

        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(
                It.IsAny<ShortCourseLearningDomainModel>(),
                It.IsAny<LearnerDomainModel>(),
                It.IsAny<long>()))
            .Returns(new CreateDraftShortCourseCommandResult());

        _featureFlags = new FeatureFlags { ShortCourseChangeOfProvider = true };

        _commandHandler = new CreateDraftShortCourseCommandHandler(
            _learnerFactory.Object,
            _learnerRepository.Object,
            _learningRepository.Object,
            _learningFactory.Object,
            _mapper.Object,
            _logger.Object,
            _featureFlags);

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
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>())).Returns(domainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
        result.LearningKey.Should().Be(domainModel.Key);
        domainModel.LatestEpisodeForProvider(command.Model.OnProgramme.Ukprn).LearningType.Should().Be(command.Model.OnProgramme.LearningType);
        AssertPersonalDetailsEvent(
            domainModel, 
            0, //ApprovalsApprenticeshipId not available on creation
            domainModel.Key,
            learnerDomainModel.FirstName,
            learnerDomainModel.LastName);

    }

    [Test]
    public async Task ThenCreatesNewEpisodeIfApprovedEpisodeExistsWithAnotherProvider()
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
        result.Should().NotBeNull();
        existingLearning.Episodes.Should().HaveCount(2);
        existingLearning.Episodes.Should().Contain(x => x.Ukprn == command.Model.OnProgramme.Ukprn);
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
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
        result.Should().BeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenCreatesNewEpisodeIfUnapprovedEpisodeExistsWithAnotherProvider()
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
        result.Should().NotBeNull();
        existingLearning.Episodes.Should().HaveCount(2);
        existingLearning.Episodes.Should().Contain(x => x.Ukprn == command.Model.OnProgramme.Ukprn);
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
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
        var result = await _commandHandler.Handle(command);

        // Assert
        existingLearning.LatestEpisodeForProvider(command.Model.OnProgramme.Ukprn).LearningType.Should().Be(command.Model.OnProgramme.LearningType);
        result.IsReinstated.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenReinstatesEpisodeIfPreviouslyRemoved()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: command.Model.OnProgramme.Ukprn, isRemoved: true);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        var mappedLearner = new ShortCourseLearnerDto { Uln = "1234567890", FirstName = "Jane", LastName = "Smith" };
        var mappedEpisodes = new[] { new ShortCourseEpisodeDto { CourseCode = "SC-001" } };
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(existingLearning, learner, command.Model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult
            {
                LearnerKey = learner.Key,
                Learner = mappedLearner,
                Episodes = mappedEpisodes
            });

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.IsReinstated.Should().BeTrue();
        result.LearnerKey.Should().Be(learner.Key);
        result.Learner.Should().Be(mappedLearner);
        result.Episodes.Should().BeEquivalentTo(mappedEpisodes);
        existingLearning.LatestEpisodeForProvider(command.Model.OnProgramme.Ukprn).IsRemoved.Should().BeFalse();

        var reinstatedEvent = existingLearning.FlushEvents().OfType<Domain.Events.LearningReinstatedEvent>().SingleOrDefault();
        reinstatedEvent.Should().NotBeNull();
        reinstatedEvent!.LearningKey.Should().Be(existingLearning.Key);
        reinstatedEvent.ApprenticeshipId.Should().Be(existingLearning.LatestEpisodeForProvider(command.Model.OnProgramme.Ukprn).ApprovalsApprenticeshipId);

        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenPersonalDetailsEventAdded_When_LearnerDetailsAreUpdated_And_LearningStillUnapproved()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: command.Model.OnProgramme.Ukprn, learningType: LearningType.ApprenticeshipUnit);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        AssertPersonalDetailsEvent(
            existingLearning,
            0, //ApprovalsApprenticeshipId not available on creation
            existingLearning.Key,
            command.Model.Learner.FirstName,
            command.Model.Learner.LastName);
    }
    [Test]
    public async Task ThenRejectsCommandIfFeatureFlagIsFalseAndDifferentProviderExists()
    {
        // Arrange
        _featureFlags.ShortCourseChangeOfProvider = false;
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: command.Model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Should().BeNull();
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenRejectsCommandIfFeatureFlagIsFalseAndApprovedEpisodeExistsForSameProvider()
    {
        // Arrange
        _featureFlags.ShortCourseChangeOfProvider = false;
        var command = _fixture.Create<CreateDraftShortCourseCommand>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: command.Model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(existingLearning);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        result.Should().BeNull();
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    private ShortCourseLearningDomainModel BuildLearningWithEpisode(bool isApproved, long ukprn, LearningType learningType = LearningType.Apprenticeship, bool isRemoved = false)
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
                    IsRemoved = isRemoved,
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

    private void AssertPersonalDetailsEvent(
        ShortCourseLearningDomainModel domainModel,
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
}


