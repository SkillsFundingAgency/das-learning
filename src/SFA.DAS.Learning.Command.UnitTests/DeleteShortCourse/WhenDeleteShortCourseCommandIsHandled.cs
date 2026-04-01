using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.DeleteShortCourse;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.DeleteShortCourse;

[TestFixture]
public class WhenDeleteShortCourseCommandIsHandled
{
    private DeleteShortCourseCommandHandler _commandHandler = null!;
    private Mock<IShortCourseLearningRepository> _repository = null!;
    private Mock<ILogger<DeleteShortCourseCommandHandler>> _logger = null!;

    private const long Ukprn = 12345678;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<DeleteShortCourseCommandHandler>>();
        _commandHandler = new DeleteShortCourseCommandHandler(_logger.Object, _repository.Object);
    }

    [Test]
    public async Task ThenWithdrawalDateIsSetToStartDate()
    {
        var startDate = new DateTime(2024, 8, 1);
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, startDate: startDate);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        learning.Episodes.Single().WithdrawalDate.Should().Be(startDate);
    }

    [Test]
    public async Task ThenRepositoryUpdateIsCalled()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        _repository.Verify(r => r.Update(learning), Times.Once);
    }

    [Test]
    public async Task ThenWasDeletedIsTrueWhenApprovedAndDeleted()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: true);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        result.WasDeleted.Should().BeTrue();
    }

    [Test]
    public async Task ThenWasDeletedIsFalseAndDoesNothingWhenUkprnDoesNotMatch()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: true);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, ukprn: 99999999));

        result.WasDeleted.Should().BeFalse();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenWasDeletedIsFalseAndDoesNothingWhenNotApproved()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: false);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        result.WasDeleted.Should().BeFalse();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenALearningDeletedEventIsRaisedWhenApprovedAndDeleted()
    {
        var learningKey = Guid.NewGuid();
        var approvalsApprenticeshipId = 42L;
        var employerAccountId = 99L;
        var startDate = new DateTime(2024, 8, 1);
        var learning = CreateDomainModel(learningKey, startDate: startDate, approvalsApprenticeshipId: approvalsApprenticeshipId, employerAccountId: employerAccountId, isApproved: true);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        learning.FlushEvents()
            .OfType<Domain.Events.LearningDeletedEvent>()
            .Should().ContainSingle(e =>
                e.LearningKey == learningKey &&
                e.ApprovalsApprenticeshipId == approvalsApprenticeshipId &&
                e.EmployerAccountId == employerAccountId &&
                e.LastDayOfLearning == startDate);
    }

    [Test]
    public async Task ThenNoLearningDeletedEventIsRaisedWhenNotApproved()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: false);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        learning.FlushEvents()
            .OfType<Domain.Events.LearningDeletedEvent>()
            .Should().BeEmpty();
    }

    [Test]
    public async Task ThenKeyNotFoundExceptionThrownWhenLearningNotFound()
    {
        var learningKey = Guid.NewGuid();
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync((ShortCourseLearningDomainModel)null!);

        var act = async () => await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static ShortCourseLearningDomainModel CreateDomainModel(
        Guid learningKey,
        DateTime? startDate = null,
        DateTime? completionDate = null,
        List<Milestone>? milestones = null,
        bool isApproved = true,
        long approvalsApprenticeshipId = 1,
        long employerAccountId = 1)
    {
        startDate ??= DateTime.Today.AddMonths(-1);
        var episodeKey = Guid.NewGuid();
        var episode = new ShortCourseEpisode
        {
            Key = episodeKey,
            LearningKey = learningKey,
            Ukprn = 12345678,
            EmployerAccountId = employerAccountId,
            ApprovalsApprenticeshipId = approvalsApprenticeshipId,
            TrainingCode = "TEST01",
            LearnerRef = "LEARNER1",
            IsApproved = isApproved,
            StartDate = startDate.Value,
            ExpectedEndDate = startDate.Value.AddMonths(6),
            Price = 1000,
            Milestones = new List<ShortCourseMilestone>()
        };

        foreach (var milestone in milestones ?? [])
            episode.Milestones.Add(new ShortCourseMilestone { Key = Guid.NewGuid(), EpisodeKey = episodeKey, Milestone = milestone });

        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            CompletionDate = completionDate,
            Episodes = new List<ShortCourseEpisode> { episode }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }
}
