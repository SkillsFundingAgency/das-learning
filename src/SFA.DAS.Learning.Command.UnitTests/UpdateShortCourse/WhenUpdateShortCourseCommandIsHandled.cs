using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Command.UpdateShortCourse;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.UpdateShortCourse;

[TestFixture]
public class WhenUpdateShortCourseCommandIsHandled
{
    private Fixture _fixture = new Fixture();
    private UpdateShortCourseCommandHandler _commandHandler = null!;
    private Mock<IShortCourseLearningRepository> _repository = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<ILogger<UpdateShortCourseCommandHandler>> _logger = null!;
    private Mock<IShortCourseLearningDomainModelMapper> _mapper = null!;
    private FeatureFlags _featureFlags = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortCourseLearningRepository>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _logger = new Mock<ILogger<UpdateShortCourseCommandHandler>>();
        _mapper = new Mock<IShortCourseLearningDomainModelMapper>();
        _featureFlags = new FeatureFlags();

        _learnerRepository
            .Setup(r => r.Get(It.IsAny<Guid>()))
            .ReturnsAsync(LearnerDomainModel.Get(new Learner
            {
                Key = Guid.NewGuid(),
                Uln = "1234567890",
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = DateTime.Today.AddYears(-20)
            }));

        _mapper.Setup(x => x.Map<UpdateShortCourseResult>(
            It.IsAny<ShortCourseLearningDomainModel>(), It.IsAny<LearnerDomainModel>(), It.IsAny<long>())
            )
            .Returns(() => _fixture.Build<UpdateShortCourseResult>().With(x => x.IsRemoved, false).Create());

        _repository
            .Setup(r => r.GetAllByLearnerKey(It.IsAny<Guid>()))
            .ReturnsAsync(new List<ShortCourseLearningDomainModel>());

        _commandHandler = new UpdateShortCourseCommandHandler(_logger.Object, _repository.Object, _learnerRepository.Object, _mapper.Object, _featureFlags, factory: null);
    }

    [Test]
    public async Task ThenUpdatedEpisodeKeyIsPopulated()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel();

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().UpdatedEpisodeKey.Should().Be(learning.Episodes.Single().Key);
    }

    [Test]
    public async Task ThenWithdrawalDateChangeIsDetected()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(withdrawalDate: null);

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext(withdrawalDate: DateTime.Today)]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().Changes.Should().Contain(ShortCourseUpdateChanges.WithdrawalDate);
    }

    [Test]
    public async Task ThenCompletionDateChangeIsDetected()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(completionDate: null);

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext(completionDate: DateTime.Today)]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().Changes.Should().Contain(ShortCourseUpdateChanges.CompletionDate);
    }

    [Test]
    public async Task ThenMilestoneChangeIsDetected()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(milestones: new List<Milestone> { Milestone.ThirtyPercentLearningComplete });

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext(milestones: new List<Milestone> { Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete })]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().Changes.Should().Contain(ShortCourseUpdateChanges.Milestone);
    }

    [Test]
    public async Task ThenLearnerRefChangeIsDetected()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel();

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext(learnerRef: "LearnerRefChanged")]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().Changes.Should().Contain(ShortCourseUpdateChanges.LearnerRef);
    }

    [Test]
    public async Task ThenNoChangesReturnedWhenNothingChanged()
    {
        var learnerKey = Guid.NewGuid();
        var withdrawalDate = DateTime.Today;
        var milestones = new List<Milestone> { Milestone.ThirtyPercentLearningComplete };
        var learning = CreateDomainModel(withdrawalDate: withdrawalDate, milestones: milestones);

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext(withdrawalDate: withdrawalDate, milestones: milestones)]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().Changes.Should().BeEmpty();
    }

    [Test]
    public async Task ThenLearningTypeIsNotChangedByPut()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(isApproved: true, learningType: LearningType.ApprenticeshipUnit);

        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var context = CreateUpdateContext();
        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [context]);

        await _commandHandler.Handle(command);

        learning.LatestEpisodeForProvider(context.OnProgramme.Ukprn).LearningType.Should().Be(LearningType.ApprenticeshipUnit);
    }

    [Test]
    public async Task ThenIgnoredResultReturnedWhenLearningNotFoundAndFlagDisabled()
    {
        var learnerKey = Guid.NewGuid();
        _featureFlags.ShortCourseProgression = false;
        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync((ShortCourseLearningDomainModel?)null);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        results.Results.Single().IsIgnored.Should().BeTrue();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenNewLearningIsCreatedWhenNoCourseCodeMatchFound()
    {
        var learnerKey = Guid.NewGuid();
        var context = CreateUpdateContext();
        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [context]);
        _featureFlags.ShortCourseProgression = true;

        _repository
            .Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, context.OnProgramme.CourseCode))
            .ReturnsAsync((ShortCourseLearningDomainModel?)null);

        var factoryMock = new Mock<IShortCourseLearningFactory>();
        factoryMock
            .Setup(f => f.CreateNew(learnerKey, context.OnProgramme.CourseCode))
            .Returns(CreateDomainModel());

        _commandHandler = new UpdateShortCourseCommandHandler(
            _logger.Object, _repository.Object, _learnerRepository.Object, _mapper.Object, _featureFlags, factoryMock.Object);

        await _commandHandler.Handle(command);

        _repository.Verify(r => r.Add(It.IsAny<ShortCourseLearningDomainModel>()), Times.Once);
    }

    [Test]
    public async Task ThenOmittedLearningIsNotRemovedWhenFlagDisabled()
    {
        var learnerKey = Guid.NewGuid();
        _featureFlags.ShortCourseProgression = false;

        var includedLearning = CreateDomainModel(courseCode: "TEST01");
        var omittedLearning = CreateDomainModel(courseCode: "TEST02", isApproved: true);

        _repository
            .Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01"))
            .ReturnsAsync(includedLearning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        results.Results.Should().NotContain(r => r.IsRemoved);
        omittedLearning.Episodes.Single().IsRemoved.Should().BeFalse();
        _repository.Verify(r => r.GetAllByLearnerKey(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task ThenOmittedUnapprovedLearningIsRemovedWhenFlagEnabled()
    {
        var learnerKey = Guid.NewGuid();
        _featureFlags.ShortCourseProgression = true;

        var includedLearning = CreateDomainModel(courseCode: "TEST01");
        var omittedLearning = CreateDomainModel(courseCode: "TEST02", isApproved: false);

        _repository
            .Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01"))
            .ReturnsAsync(includedLearning);
        _repository
            .Setup(r => r.GetAllByLearnerKey(learnerKey))
            .ReturnsAsync([includedLearning, omittedLearning]);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        results.Results.Should().Contain(r => r.IsRemoved && r.CourseCode == "TEST02");
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        _repository.Verify(r => r.Update(omittedLearning), Times.Once);
    }

    [Test]
    public async Task ThenOmittedApprovedLearningIsRemovedWhenFlagEnabled()
    {
        var learnerKey = Guid.NewGuid();
        _featureFlags.ShortCourseProgression = true;

        var includedLearning = CreateDomainModel(courseCode: "TEST01");
        var omittedLearning = CreateDomainModel(courseCode: "TEST02", isApproved: true);

        _repository
            .Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01"))
            .ReturnsAsync(includedLearning);
        _repository
            .Setup(r => r.GetAllByLearnerKey(learnerKey))
            .ReturnsAsync([includedLearning, omittedLearning]);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        results.Results.Should().Contain(r => r.IsRemoved && r.CourseCode == "TEST02");
        omittedLearning.Episodes.Single().IsRemoved.Should().BeTrue();
        _repository.Verify(r => r.Update(omittedLearning), Times.Once);
    }

    [Test]
    public async Task ThenRemovedUnapprovedEpisodeIsReinstatedWhenItReappearsInPut()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(isApproved: false, isRemoved: true);
        _repository.Setup(r => r.GetByLearnerKeyAndCourseCode(learnerKey, "TEST01")).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learnerKey, 12345678, [CreateUpdateContext()]);

        var results = await _commandHandler.Handle(command);

        learning.Episodes.Single().IsRemoved.Should().BeFalse();
        results.Results.Single().Changes.Should().Contain(ShortCourseUpdateChanges.Reinstated);
        learning.FlushEvents().Should().NotContain(e => e is LearningReinstatedEvent);
    }

    private static ShortCourseLearningDomainModel CreateDomainModel(DateTime? withdrawalDate = null, List<Milestone>? milestones = null, DateTime? completionDate = null, bool isApproved = false, bool isRemoved = false, LearningType learningType = LearningType.Apprenticeship, string courseCode = "TEST01", long ukprn = 12345678)
    {
        var learningKey = Guid.NewGuid();
        var episodeKey = Guid.NewGuid();
        var episode = new ShortCourseEpisode
        {
            Key = episodeKey,
            LearningKey = learningKey,
            Ukprn = ukprn,
            EmployerAccountId = 1,
            TrainingCode = courseCode,
            LearnerRef = "LEARNER1",
            IsApproved = isApproved,
            IsRemoved = isRemoved,
            StartDate = DateTime.Today.AddMonths(-1),
            ExpectedEndDate = DateTime.Today.AddMonths(6),
            WithdrawalDate = withdrawalDate,
            Price = 1000,
            LearningType = learningType,
            Milestones = new List<ShortCourseMilestone>()
        };

        foreach (var milestone in milestones ?? new List<Milestone>())
        {
            episode.Milestones.Add(new ShortCourseMilestone { Key = Guid.NewGuid(), EpisodeKey = episodeKey, Milestone = milestone });
        }

        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            LearnerKey = Guid.NewGuid(),
            TrainingCode = courseCode,
            CompletionDate = completionDate,
            Episodes = new List<ShortCourseEpisode> { episode }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }

    private static ShortCourseUpdateContext CreateUpdateContext(
        DateTime? withdrawalDate = null,
        List<Milestone>? milestones = null,
        DateTime? completionDate = null,
        string learnerRef = "LEARNER1")
    {
        return new ShortCourseUpdateContext
        {
            LearnerRef = learnerRef,
            Learner = new LearnerModel { Uln = "1234567890", FirstName = "Test", LastName = "User", DateOfBirth = DateTime.Today.AddYears(-20) },
            LearningSupport = new List<Models.UpdateModels.Shared.LearningSupportDetails>(),
            OnProgramme = new Models.UpdateModels.OnProgramme
            {
                CourseCode = "TEST01",
                EmployerId = 1,
                Ukprn = 12345678,
                StartDate = DateTime.Today.AddMonths(-1),
                ExpectedEndDate = DateTime.Today.AddMonths(6),
                WithdrawalDate = withdrawalDate,
                CompletionDate = completionDate,
                Milestones = milestones ?? new List<Milestone>(),
                Price = 1000
            }
        };
    }
}
