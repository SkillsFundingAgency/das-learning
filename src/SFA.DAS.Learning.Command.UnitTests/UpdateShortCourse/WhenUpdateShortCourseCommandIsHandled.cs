using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.UpdateShortCourse;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.UpdateShortCourse;

[TestFixture]
public class WhenUpdateShortCourseCommandIsHandled
{
    private UpdateShortCourseCommandHandler _commandHandler = null!;
    private Mock<IShortCourseLearningRepository> _repository = null!;
    private Mock<ILogger<UpdateShortCourseCommandHandler>> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<UpdateShortCourseCommandHandler>>();
        _commandHandler = new UpdateShortCourseCommandHandler(_logger.Object, _repository.Object);
    }

    [Test]
    public async Task ThenWithdrawalDateChangeIsDetected()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, withdrawalDate: null);

        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learningKey, CreateUpdateContext(withdrawalDate: DateTime.Today));

        var result = await _commandHandler.Handle(command);

        result.Changes.Should().Contain(ShortCourseUpdateChanges.WithdrawalDate);
        result.LearningKey.Should().Be(learningKey);
    }

    [Test]
    public async Task ThenCompletionDateChangeIsDetected()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, completionDate: null);

        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learningKey, CreateUpdateContext(completionDate: DateTime.Today));

        var result = await _commandHandler.Handle(command);

        result.Changes.Should().Contain(ShortCourseUpdateChanges.CompletionDate);
    }

    [Test]
    public async Task ThenMilestoneChangeIsDetected()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, milestones: new List<Milestone> { Milestone.ThirtyPercentLearningComplete });

        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learningKey, CreateUpdateContext(milestones: new List<Milestone> { Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete }));

        var result = await _commandHandler.Handle(command);

        result.Changes.Should().Contain(ShortCourseUpdateChanges.Milestone);
    }

    [Test]
    public async Task ThenNoChangesReturnedWhenNothingChanged()
    {
        var learningKey = Guid.NewGuid();
        var withdrawalDate = DateTime.Today;
        var milestones = new List<Milestone> { Milestone.ThirtyPercentLearningComplete };
        var learning = CreateDomainModel(learningKey, withdrawalDate: withdrawalDate, milestones: milestones);

        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var command = new UpdateShortCourseCommand(learningKey, CreateUpdateContext(withdrawalDate: withdrawalDate, milestones: milestones));

        var result = await _commandHandler.Handle(command);

        result.Changes.Should().BeEmpty();
    }

    [Test]
    public async Task ThenKeyNotFoundExceptionThrownWhenLearningNotFound()
    {
        var learningKey = Guid.NewGuid();
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync((ShortCourseLearningDomainModel)null!);

        var command = new UpdateShortCourseCommand(learningKey, CreateUpdateContext());

        var act = async () => await _commandHandler.Handle(command);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static ShortCourseLearningDomainModel CreateDomainModel(Guid learningKey, DateTime? withdrawalDate = null, List<Milestone>? milestones = null, DateTime? completionDate = null)
    {
        var episodeKey = Guid.NewGuid();
        var episode = new ShortCourseEpisode
        {
            Key = episodeKey,
            LearningKey = learningKey,
            Ukprn = 12345678,
            EmployerAccountId = 1,
            TrainingCode = "TEST01",
            IsApproved = false,
            StartDate = DateTime.Today.AddMonths(-1),
            ExpectedEndDate = DateTime.Today.AddMonths(6),
            WithdrawalDate = withdrawalDate,
            Price = 1000,
            Milestones = new List<ShortCourseMilestone>()
        };

        foreach (var milestone in milestones ?? new List<Milestone>())
        {
            episode.Milestones.Add(new ShortCourseMilestone { Key = Guid.NewGuid(), EpisodeKey = episodeKey, Milestone = milestone });
        }

        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            CompletionDate = completionDate,
            Episodes = new List<ShortCourseEpisode> { episode }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }

    private static ShortCourseUpdateContext CreateUpdateContext(DateTime? withdrawalDate = null, List<Milestone>? milestones = null, DateTime? completionDate = null)
    {
        return new ShortCourseUpdateContext
        {
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
