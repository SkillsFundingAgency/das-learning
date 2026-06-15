using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.RemoveShortCourse;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.RemoveShortCourse;

[TestFixture]
public class WhenRemoveShortCourseCommandIsHandled
{
    private Fixture _fixture = new Fixture();
    private RemoveShortCourseCommandHandler _commandHandler = null!;
    private Mock<IShortCourseLearningRepository> _repository = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IShortCourseLearningDomainModelMapper> _mapper = null!;
    private Mock<ILogger<RemoveShortCourseCommandHandler>> _logger = null!;

    private const long Ukprn = 12345678;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<RemoveShortCourseCommandHandler>>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _mapper = new Mock<IShortCourseLearningDomainModelMapper>();

        _learnerRepository.Setup(r => r.Get(It.IsAny<Guid>())).ReturnsAsync(CreateLearnerDomainModel());

        _mapper.Setup(x => x.Map<RemoveShortCourseResult>(
            It.IsAny<ShortCourseLearningDomainModel>(), It.IsAny<LearnerDomainModel>(), It.IsAny<long>())
            )
            .Returns(_fixture.Create<RemoveShortCourseResult>());

        _commandHandler = new RemoveShortCourseCommandHandler(_logger.Object, _repository.Object, _learnerRepository.Object, _mapper.Object);
    }

    [Test]
    public async Task ThenIsRemovedIsSetToTrue()
    {
        var learnerKey = Guid.NewGuid();
        var startDate = new DateTime(2024, 8, 1);
        var learning = CreateDomainModel(Guid.NewGuid(), startDate: startDate);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        learning.Episodes.Single().IsRemoved.Should().BeTrue();
    }

    [Test]
    public async Task ThenRepositoryUpdateIsCalled()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(Guid.NewGuid());
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        _repository.Verify(r => r.Update(learning), Times.Once);
    }

    [Test]
    public async Task ThenWasDeletedIsTrueWhenApprovedAndDeleted()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(Guid.NewGuid(), isApproved: true);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenRemovedEpisodeKeyIsPopulated()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(Guid.NewGuid(), isApproved: true);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        var result = await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        result.Should().NotBeNull();
        result!.RemovedEpisodeKey.Should().Be(learning.Episodes.Single().Key);
    }

    [Test]
    public async Task ThenNotFoundExceptionThrownWhenUkprnDoesNotMatch()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(Guid.NewGuid(), isApproved: true);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        var act = async () => await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, ukprn: 99999999));

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenNotFoundExceptionThrownWhenNotApproved()
    {
        var learnerKey = Guid.NewGuid();
        var learning = CreateDomainModel(Guid.NewGuid(), isApproved: false);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        var act = async () => await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task ThenALearningRemovedEventIsRaisedWhenApprovedAndDeleted()
    {
        var learnerKey = Guid.NewGuid();
        var learningKey = Guid.NewGuid();
        var approvalsApprenticeshipId = 42L;
        var employerAccountId = 99L;
        var startDate = new DateTime(2024, 8, 1);
        var learning = CreateDomainModel(learningKey, startDate: startDate, approvalsApprenticeshipId: approvalsApprenticeshipId, employerAccountId: employerAccountId, isApproved: true);
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync(learning);

        await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        learning.FlushEvents()
            .OfType<Domain.Events.LearningRemovedEvent>()
            .Should().ContainSingle(e =>
                e.LearningKey == learningKey &&
                e.ApprenticeshipId == approvalsApprenticeshipId);
    }

    [Test]
    public async Task ThenNotFoundExceptionThrownWhenLearningNotFound()
    {
        var learnerKey = Guid.NewGuid();
        _repository.Setup(r => r.GetByLearnerKey(learnerKey)).ReturnsAsync((ShortCourseLearningDomainModel?)null);

        var act = async () => await _commandHandler.Handle(new RemoveShortCourseCommand(learnerKey, Ukprn));

        await act.Should().ThrowAsync<NotFoundException>();
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
            TrainingCode = "SC001",
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
