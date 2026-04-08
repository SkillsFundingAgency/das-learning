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
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<ILogger<DeleteShortCourseCommandHandler>> _logger = null!;

    private const long Ukprn = 12345678;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<DeleteShortCourseCommandHandler>>();
        _learnerRepository = new Mock<ILearnerRepository>();

        _learnerRepository.Setup(r => r.Get(It.IsAny<Guid>())).ReturnsAsync(CreateLearnerDomainModel());
        _commandHandler = new DeleteShortCourseCommandHandler(_logger.Object, _repository.Object, _learnerRepository.Object);
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
    public async Task ThenReturnsTrueWhenApprovedAndDeleted()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: true);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenReturnsFalseAndDoesNothingWhenUkprnDoesNotMatch()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: true);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, ukprn: 99999999));

        result.Should().BeNull();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenReturnsFalseAndDoesNothingWhenNotApproved()
    {
        var learningKey = Guid.NewGuid();
        var learning = CreateDomainModel(learningKey, isApproved: false);
        _repository.Setup(r => r.Get(learningKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new DeleteShortCourseCommand(learningKey, Ukprn));

        result.Should().BeNull();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
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
        bool isApproved = true)
    {
        startDate ??= DateTime.Today.AddMonths(-1);
        var episodeKey = Guid.NewGuid();
        var episode = new ShortCourseEpisode
        {
            Key = episodeKey,
            LearningKey = learningKey,
            Ukprn = 12345678,
            EmployerAccountId = 1,
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

    private static LearnerDomainModel CreateLearnerDomainModel()
    {
        return LearnerDomainModel.Get(new Learner
        {
            Key = Guid.NewGuid(),
            Uln = "1234567890",
            FirstName = "John",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 1, 1)
        });
    }
}
