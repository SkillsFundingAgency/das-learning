using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.ApproveLearningChange;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.ApproveLearningChange;

[TestFixture]
public class WhenAnApproveLearningChangeCommandIsSent
{
    private static readonly DateTime StartDate = new(2025, 8, 1);

    private ApproveLearningChangeCommandHandler _handler = null!;
    private Mock<ILearningService> _learningService = null!;
    private ApprenticeshipEpisode _episode = null!;
    private ApprenticeshipLearningDomainModel _learning = null!;

    [SetUp]
    public void SetUp()
    {
        _learningService = new Mock<ILearningService>();
        _handler = new ApproveLearningChangeCommandHandler(
            _learningService.Object,
            Mock.Of<ILogger<ApproveLearningChangeCommandHandler>>());

        _episode = CreateEpisode(furtherApprovalNeeded: true);
        _learning = CreateLearning(_episode);

        _learningService
            .Setup(x => x.GetLearning(_learning.Key, LearningType.Apprenticeship))
            .ReturnsAsync(_learning);
    }

    [Test]
    public async Task AndTheStartDateChangeMatchesThenTheFlagIsClearedAndTheLearningIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learningService.Verify(x => x.UpdateLearning(_learning), Times.Once);
    }

    [Test]
    public async Task AndTheLearningIsNotFoundThenNothingIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.LearningKey = Guid.NewGuid();

        await _handler.Handle(command);

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task AndNoEpisodeMatchesTheApprenticeshipIdThenNothingIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.ApprenticeshipId = 99999;

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    private ApproveLearningChangeCommand CreateCommand(string newStartDate) => new()
    {
        LearningKey = _learning.Key,
        ApprenticeshipId = _episode.ApprovalsApprenticeshipId,
        Changes = [new ApprovedChange("StartDate", "2025-07-01", newStartDate)]
    };

    private static ApprenticeshipEpisode CreateEpisode(bool furtherApprovalNeeded)
    {
        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            ApprovalsApprenticeshipId = 12345,
            IsApproved = true,
            FurtherApprovalNeeded = furtherApprovalNeeded,
            LegalEntityName = "Test",
            LearningSupport = new List<ApprenticeshipLearningSupport>(),
            BreaksInLearning = new List<EpisodeBreakInLearning>()
        };

        episode.Prices = new List<EpisodePrice>
        {
            new()
            {
                Key = Guid.NewGuid(),
                EpisodeKey = episode.Key,
                StartDate = StartDate,
                EndDate = new DateTime(2026, 7, 31),
                TrainingPrice = 1000,
                EndPointAssessmentPrice = 200,
                TotalPrice = 1200
            }
        };

        return episode;
    }

    private static ApprenticeshipLearningDomainModel CreateLearning(params ApprenticeshipEpisode[] episodes)
    {
        var entity = new ApprenticeshipLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = Guid.NewGuid(),
            Episodes = [.. episodes],
            EnglishAndMathsCourses = new List<EnglishAndMaths>()
        };

        foreach (var episode in episodes)
        {
            episode.LearningKey = entity.Key;
        }

        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
