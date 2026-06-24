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
using SFA.DAS.Learning.Models.UpdateModels;
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

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(It.IsAny<Guid>()))
            .ReturnsAsync(new List<ShortCourseLearningDomainModel>());

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

    private CreateDraftShortCourseCommand CreateSingleItemCommand(out ShortCourseUpdateContext model)
    {
        model = _fixture.Create<ShortCourseUpdateContext>();
        return new CreateDraftShortCourseCommand(model.OnProgramme.Ukprn, [model]);
    }

    [Test]
    public async Task ThenANewShortCourseLearningIsCreated()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learningEntity = _fixture.Create<ShortCourseLearning>();
        learningEntity.Episodes = new List<ShortCourseEpisode>();


        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();

        var domainModel = ShortCourseLearningDomainModel.Get(learningEntity);

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string?>())).Returns(learnerDomainModel);
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), It.IsAny<string>())).Returns(domainModel);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        var result = results.Results.Single();
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
        result.LearningKey.Should().Be(domainModel.Key);
        domainModel.LatestEpisodeForProvider(model.OnProgramme.Ukprn).LearningType.Should().Be(model.OnProgramme.LearningType);
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
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().HaveCount(1);
        existingLearning.Episodes.Should().HaveCount(2);
        existingLearning.Episodes.Should().Contain(x => x.Ukprn == model.OnProgramme.Ukprn);
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenShortCircuitsIfApprovedEpisodeExistsWithSameProvider()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().BeEmpty();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenNoOpRepeatPostOfApprovedSameProviderCourseIsNotRemovedByOmission()
    {
        // Arrange - re-POSTing the exact same, already-approved course (same provider) results in a no-op
        // A bug resulted in this no-op being treated as an omission, and the existing Episode being removed. This test ensures that does not happen.
        _featureFlags.ShortCourseProgression = true;
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn, courseCode: model.OnProgramme.CourseCode);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([existingLearning]);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().BeEmpty();
        existingLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Never);
    }

    [Test]
    public async Task ThenCreatesNewEpisodeIfUnapprovedEpisodeExistsWithAnotherProvider()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().HaveCount(1);
        existingLearning.Episodes.Should().HaveCount(2);
        existingLearning.Episodes.Should().Contain(x => x.Ukprn == model.OnProgramme.Ukprn);
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenLearningTypeIsUpdatedWhenUpdatingExistingLearning()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn, learningType: LearningType.Apprenticeship);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        var result = results.Results.Single();
        existingLearning.LatestEpisodeForProvider(model.OnProgramme.Ukprn).LearningType.Should().Be(model.OnProgramme.LearningType);
        result.IsReinstated.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenReinstatesEpisodeIfPreviouslyRemoved()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn, isRemoved: true);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        var mappedLearner = new ShortCourseLearnerDto { Uln = "1234567890", FirstName = "Jane", LastName = "Smith" };
        var mappedEpisodes = new[] { new ShortCourseEpisodeDto { CourseCode = "SC-001" } };
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(existingLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult
            {
                LearnerKey = learner.Key,
                Learner = mappedLearner,
                Episodes = mappedEpisodes
            });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        var result = results.Results.Single();
        result.IsReinstated.Should().BeTrue();
        result.LearnerKey.Should().Be(learner.Key);
        result.Learner.Should().Be(mappedLearner);
        result.Episodes.Should().BeEquivalentTo(mappedEpisodes);
        existingLearning.LatestEpisodeForProvider(model.OnProgramme.Ukprn).IsRemoved.Should().BeFalse();

        var reinstatedEvent = existingLearning.FlushEvents().OfType<Domain.Events.LearningReinstatedEvent>().SingleOrDefault();
        reinstatedEvent.Should().NotBeNull();
        reinstatedEvent!.LearningKey.Should().Be(existingLearning.Key);
        reinstatedEvent.ApprenticeshipId.Should().Be(existingLearning.LatestEpisodeForProvider(model.OnProgramme.Ukprn).ApprovalsApprenticeshipId);

        _learningRepository.Verify(x => x.Update(existingLearning), Times.Once);
    }

    [Test]
    public async Task ThenPersonalDetailsEventAdded_When_LearnerDetailsAreUpdated_And_LearningStillUnapproved()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());

        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn, learningType: LearningType.ApprenticeshipUnit);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        AssertPersonalDetailsEvent(
            existingLearning,
            0, //ApprovalsApprenticeshipId not available on creation
            existingLearning.Key,
            model.Learner.FirstName,
            model.Learner.LastName);
    }

    [Test]
    public async Task ThenCreatesNewLearningForCourseCodeRatherThanFindingUnrelatedLearningForSameLearner()
    {
        // Arrange - learner already has a Learning for a *different* CourseCode (e.g. from a prior Progression POST).
        // The lookup must be scoped by CourseCode, not just LearnerKey, or this POST will incorrectly
        // find and mutate the unrelated Learning instead of creating a new one.
        _featureFlags.ShortCourseProgression = true;
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var unrelatedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKey(learner.Key)).ReturnsAsync(unrelatedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([unrelatedLearning]);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);

        var newLearningEntity = _fixture.Create<ShortCourseLearning>();
        newLearningEntity.Episodes = new List<ShortCourseEpisode>();
        var newDomainModel = ShortCourseLearningDomainModel.Get(newLearningEntity);
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), model.OnProgramme.CourseCode)).Returns(newDomainModel);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == newDomainModel)), Times.Once);
        results.Results.Should().Contain(r => r.LearningKey == newDomainModel.Key);

        // The unrelated Learning is omitted from this bundle, so it is removed (full-state-upsert semantics, same as PUT).
        _learningRepository.Verify(x => x.Update(unrelatedLearning), Times.Once);
        unrelatedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
    }

    [Test]
    public async Task ThenIgnoresNewCourseCodeWhenProgressionFlagDisabledAndLearnerHasOtherLearnings()
    {
        // Arrange - mirrors PUT's IsIgnored behaviour: a CourseCode with no existing Learning, but the
        // learner already has at least one other Learning, is Progression and must stay gated behind the flag.
        _featureFlags.ShortCourseProgression = false;
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var otherLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([otherLearning]);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Single().IsIgnored.Should().BeTrue();
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenHandlesBundledPostWithOriginalUnapprovedCourseAndNewProgressionCourse()
    {
        // Arrange - AC3/AC4 shape: SLD bundles the still-unapproved original course alongside the new one in a single POST.
        _featureFlags.ShortCourseProgression = true;
        var originalModel = _fixture.Create<ShortCourseUpdateContext>();
        var newModel = _fixture.Create<ShortCourseUpdateContext>();
        var command = new CreateDraftShortCourseCommand(originalModel.OnProgramme.Ukprn, [originalModel, newModel]);

        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var originalLearning = BuildLearningWithEpisode(isApproved: false, ukprn: originalModel.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, originalModel.OnProgramme.CourseCode)).ReturnsAsync(originalLearning);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, newModel.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([originalLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(originalLearning, learner, originalModel.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult { LearningKey = originalLearning.Key, LearnerKey = learner.Key });

        var newLearningEntity = _fixture.Create<ShortCourseLearning>();
        newLearningEntity.Episodes = new List<ShortCourseEpisode>();
        var newDomainModel = ShortCourseLearningDomainModel.Get(newLearningEntity);
        _learningFactory.Setup(x => x.CreateNew(learner.Key, newModel.OnProgramme.CourseCode)).Returns(newDomainModel);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().HaveCount(2);
        _learningRepository.Verify(x => x.Update(originalLearning), Times.Once);
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == newDomainModel)), Times.Once);
        results.Results.Should().Contain(r => r.LearningKey == newDomainModel.Key);
    }

    [Test]
    public async Task ThenRejectsCommandIfFeatureFlagIsFalseAndDifferentProviderExists()
    {
        // Arrange
        _featureFlags.ShortCourseChangeOfProvider = false;
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn + 1);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().BeEmpty();
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenRejectsCommandIfFeatureFlagIsFalseAndApprovedEpisodeExistsForSameProvider()
    {
        // Arrange
        _featureFlags.ShortCourseChangeOfProvider = false;
        var command = CreateSingleItemCommand(out var model);
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        var existingLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(existingLearning);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().BeEmpty();
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenOmittedLearningIsNotRemovedWhenFlagDisabled()
    {
        // Arrange
        _featureFlags.ShortCourseProgression = false;
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var includedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        var omittedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(includedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([includedLearning, omittedLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(includedLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult { LearningKey = includedLearning.Key, LearnerKey = learner.Key });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().NotContain(r => r.IsRemoved);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        includedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(omittedLearning), Times.Never);
    }

    [Test]
    public async Task ThenOmittedUnapprovedLearningIsRemovedWhenFlagEnabled()
    {
        // Arrange
        _featureFlags.ShortCourseProgression = true;
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var includedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        var omittedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(includedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([includedLearning, omittedLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(includedLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult { LearningKey = includedLearning.Key, LearnerKey = learner.Key });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().Contain(r => r.IsRemoved && r.CourseCode == omittedLearning.TrainingCode);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        includedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(omittedLearning), Times.Once);
    }

    [Test]
    public async Task ThenOmittedApprovedLearningIsRemovedWhenFlagEnabled()
    {
        // Arrange
        _featureFlags.ShortCourseProgression = true;
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var includedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        var omittedLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(includedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([includedLearning, omittedLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseCommandResult>(includedLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseCommandResult { LearningKey = includedLearning.Key, LearnerKey = learner.Key });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().Contain(r => r.IsRemoved && r.CourseCode == omittedLearning.TrainingCode);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        includedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(omittedLearning), Times.Once);
    }

    private ShortCourseLearningDomainModel BuildLearningWithEpisode(bool isApproved, long ukprn, LearningType learningType = LearningType.Apprenticeship, bool isRemoved = false, string courseCode = "SC001")
    {
        var learningKey = Guid.NewGuid();
        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            LearnerKey = Guid.NewGuid(),
            TrainingCode = courseCode,
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

