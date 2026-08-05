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
using SFA.DAS.Learning.Command.Shared;
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

        _mapper.Setup(x => x.Map<CreateDraftShortCourseItemResult>(
                It.IsAny<ShortCourseLearningDomainModel>(),
                It.IsAny<LearnerDomainModel>(),
                It.IsAny<long>()))
            .Returns((ShortCourseLearningDomainModel learning, LearnerDomainModel learner, long ukprn) =>
                new CreateDraftShortCourseItemResult
                {
                    LearningKey = learning.Key,
                    LearnerKey = learner.Key,
                    Episode = new ShortCourseCommandEpisode { CourseCode = learning.TrainingCode }
                });

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
        return new CreateDraftShortCourseCommand(model.OnProgramme.Ukprn, 2526, [model]);
    }

    [Test]
    public async Task ThenANewShortCourseLearningIsCreated()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learningEntity = _fixture.Create<ShortCourseLearning>();
        learningEntity.Episodes = new List<ShortCourseEpisode>();
        learningEntity.LearningType = LearningType.ApprenticeshipUnit;


        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();

        var domainModel = ShortCourseLearningDomainModel.Get(learningEntity);

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string?>())).Returns(learnerDomainModel);
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<LearningType>())).Returns(domainModel);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        var result = results.Results.Single();
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
        result.LearningKey.Should().Be(domainModel.Key);
        domainModel.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
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
        existingLearning.LearningType.Should().Be(model.OnProgramme.LearningType);
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

        var mappedLearner = new ShortCourseLearner { Uln = "1234567890", FirstName = "Jane", LastName = "Smith" };
        var mappedEpisode = new ShortCourseCommandEpisode { CourseCode = "SC-001" };
        _mapper.Setup(x => x.Map<CreateDraftShortCourseItemResult>(existingLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseItemResult
            {
                LearnerKey = learner.Key,
                Learner = mappedLearner,
                Episode = mappedEpisode
            });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        var result = results.Results.Single();
        result.IsReinstated.Should().BeTrue();
        result.LearnerKey.Should().Be(learner.Key);
        result.Learner.Should().Be(mappedLearner);
        result.Episode.Should().BeEquivalentTo(mappedEpisode);
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
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), model.OnProgramme.CourseCode, model.OnProgramme.Price, model.OnProgramme.LearningType)).Returns(newDomainModel);

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
    public async Task ThenHandlesBundledPostWithOriginalUnapprovedCourseAndNewProgressionCourse()
    {
        // Arrange - AC3/AC4 shape: SLD bundles the still-unapproved original course alongside the new one in a single POST.
        var originalModel = _fixture.Create<ShortCourseUpdateContext>();
        var newModel = _fixture.Create<ShortCourseUpdateContext>();
        var command = new CreateDraftShortCourseCommand(originalModel.OnProgramme.Ukprn, 2526, [originalModel, newModel]);

        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var originalLearning = BuildLearningWithEpisode(isApproved: false, ukprn: originalModel.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, originalModel.OnProgramme.CourseCode)).ReturnsAsync(originalLearning);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, newModel.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([originalLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseItemResult>(originalLearning, learner, originalModel.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseItemResult { LearningKey = originalLearning.Key, LearnerKey = learner.Key });

        var newLearningEntity = _fixture.Create<ShortCourseLearning>();
        newLearningEntity.Episodes = new List<ShortCourseEpisode>();
        var newDomainModel = ShortCourseLearningDomainModel.Get(newLearningEntity);
        _learningFactory.Setup(x => x.CreateNew(learner.Key, newModel.OnProgramme.CourseCode, newModel.OnProgramme.Price, newModel.OnProgramme.LearningType)).Returns(newDomainModel);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().HaveCount(2);
        _learningRepository.Verify(x => x.Update(originalLearning), Times.Once);
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == newDomainModel)), Times.Once);
        results.Results.Should().Contain(r => r.LearningKey == newDomainModel.Key);
    }

    [Test]
    public async Task ThenIgnoresSecondItemWithSameCourseCodeWithinABundledPost()
    {
        // Arrange - a single bundled POST contains two items for the same CourseCode and provider
        // The second must be ignored, until Restarts and Repeats are implemented

        var model1 = _fixture.Create<ShortCourseUpdateContext>();
        var model2 = _fixture.Create<ShortCourseUpdateContext>();
        model2.OnProgramme = new OnProgramme
        {
            CourseCode = model1.OnProgramme.CourseCode,
            Ukprn = model1.OnProgramme.Ukprn,
            EmployerId = model1.OnProgramme.EmployerId,
            StartDate = model1.OnProgramme.StartDate.AddMonths(6),
            ExpectedEndDate = model1.OnProgramme.ExpectedEndDate.AddMonths(6),
            WithdrawalDate = null,
            WithdrawalReasonCode = null,
            CompletionDate = null,
            Milestones = new List<Milestone>(),
            Price = model1.OnProgramme.Price,
            LearningType = model1.OnProgramme.LearningType
        };

        var command = new CreateDraftShortCourseCommand(model1.OnProgramme.Ukprn, 2526, [model1, model2]);

        var learner = _fixture.Create<LearnerDomainModel>();
        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(learner);

        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model1.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);

        var learningEntity1 = _fixture.Create<ShortCourseLearning>();
        learningEntity1.Episodes = new List<ShortCourseEpisode>();
        var domainModel1 = ShortCourseLearningDomainModel.Get(learningEntity1);

        _learningFactory.Setup(x => x.CreateNew(learner.Key, model1.OnProgramme.CourseCode, model1.OnProgramme.Price, model1.OnProgramme.LearningType)).Returns(domainModel1);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert - only one Learning is ever created for this CourseCode, and it is untouched by item 2
        _learningFactory.Verify(x => x.CreateNew(learner.Key, model1.OnProgramme.CourseCode, model1.OnProgramme.Price, model1.OnProgramme.LearningType), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        domainModel1.Episodes.Should().HaveCount(1);
        domainModel1.Episodes.Single().StartDate.Should().Be(model1.OnProgramme.StartDate);
        
        results.Results.Should().HaveCount(2);
        results.Results.Should().Contain(r => r.LearningKey == domainModel1.Key);
        results.Results.Should().Contain(r => r.IsIgnored);
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
    public async Task ThenOmittedUnapprovedLearningIsRemoved()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var includedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        var omittedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(includedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([includedLearning, omittedLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseItemResult>(includedLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseItemResult { LearningKey = includedLearning.Key, LearnerKey = learner.Key });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().Contain(r => r.IsRemoved && r.Episode!.CourseCode == omittedLearning.TrainingCode);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        includedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(omittedLearning), Times.Once);
    }

    [Test]
    public async Task ThenOmittedApprovedLearningIsRemoved()
    {
        // Arrange
        var command = CreateSingleItemCommand(out var model);
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var includedLearning = BuildLearningWithEpisode(isApproved: false, ukprn: model.OnProgramme.Ukprn);
        var omittedLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync(includedLearning);
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([includedLearning, omittedLearning]);
        _mapper.Setup(x => x.Map<CreateDraftShortCourseItemResult>(includedLearning, learner, model.OnProgramme.Ukprn))
            .Returns(new CreateDraftShortCourseItemResult { LearningKey = includedLearning.Key, LearnerKey = learner.Key });

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().Contain(r => r.IsRemoved && r.Episode!.CourseCode == omittedLearning.TrainingCode);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        includedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(omittedLearning), Times.Once);
    }

    [Test]
    public async Task ThenPriorAYCompletedLearningIsNotRemovedByOmissionInSubsequentAY()
    {
        // Arrange: learner completed a course in AY 2425, now starts a different course in AY 2526.
        // The prior-AY learning is not in the POST payload, but must not be treated as a candidate for removal
        var command = CreateSingleItemCommand(out var model); // AY 2526
        var learner = LearnerDomainModel.Get(_fixture.Create<Learner>());
        _learnerRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(learner);

        var newLearningEntity = _fixture.Create<ShortCourseLearning>();
        newLearningEntity.Episodes = new List<ShortCourseEpisode>();
        var newLearning = ShortCourseLearningDomainModel.Get(newLearningEntity);
        _learningRepository.Setup(x => x.GetByLearnerKeyAndCourseCode(learner.Key, model.OnProgramme.CourseCode)).ReturnsAsync((ShortCourseLearningDomainModel?)null);
        _learningFactory.Setup(x => x.CreateNew(learner.Key, model.OnProgramme.CourseCode, model.OnProgramme.Price, model.OnProgramme.LearningType)).Returns(newLearning);

        // Prior AY episode: completed before AY 2526 begins (i.e., CompletionDate < 2025-08-01)
        var priorAYLearning = BuildLearningWithEpisode(isApproved: true, ukprn: model.OnProgramme.Ukprn,
            startDate: new DateTime(2024, 9, 1), completionDate: new DateTime(2025, 7, 1));
        _learningRepository.Setup(x => x.GetAllByLearnerKey(learner.Key)).ReturnsAsync([priorAYLearning]);

        // Act
        var results = await _commandHandler.Handle(command);

        // Assert
        results.Results.Should().NotContain(r => r.IsRemoved);
        priorAYLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(priorAYLearning), Times.Never);
    }

    private ShortCourseLearningDomainModel BuildLearningWithEpisode(
        bool isApproved,
        long ukprn,
        LearningType learningType = LearningType.Apprenticeship,
        bool isRemoved = false,
        string courseCode = "SC001",
        DateTime? startDate = null,
        DateTime? completionDate = null)
    {
        var learningKey = Guid.NewGuid();
        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            LearnerKey = Guid.NewGuid(),
            TrainingCode = courseCode,
            LearningType = learningType,
            Price = 1000,
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
                    StartDate = startDate ?? new DateTime(2025, 9, 1),
                    ExpectedEndDate = new DateTime(2025, 12, 31),
                    CompletionDate = completionDate,
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

