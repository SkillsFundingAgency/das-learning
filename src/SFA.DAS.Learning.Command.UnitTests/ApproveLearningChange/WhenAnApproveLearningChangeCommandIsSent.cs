using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.ApproveLearningChange;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using EmployerApprovalAppliedEvent = SFA.DAS.Learning.Domain.Events.EmployerApprovalAppliedEvent;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.ApproveLearningChange;

[TestFixture]
public class WhenAnApproveLearningChangeCommandIsSent
{
    private static readonly DateTime StartDate = new(2025, 8, 1);
    private static readonly DateTime LaterPriceDate = new(2026, 2, 1);

    private ApproveLearningChangeCommandHandler _handler = null!;
    private Mock<ILearningService> _learningService = null!;
    private Mock<ILogger<ApproveLearningChangeCommandHandler>> _logger = null!;
    private ApprenticeshipEpisode _episode = null!;
    private ApprenticeshipLearningDomainModel _learning = null!;

    [SetUp]
    public void SetUp()
    {
        _learningService = new Mock<ILearningService>();
        _logger = new Mock<ILogger<ApproveLearningChangeCommandHandler>>();
        _handler = new ApproveLearningChangeCommandHandler(
            _learningService.Object,
            _logger.Object);

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
    public async Task AndTheStartDateChangeMatchesThenAnEmployerApprovalAppliedEventIsRaised()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");

        await _handler.Handle(command);

        var raisedEvent = _learning.FlushEvents().OfType<EmployerApprovalAppliedEvent>().Single();
        raisedEvent.EpisodeKey.Should().Be(_episode.Key);
        raisedEvent.Changes.Should().BeEquivalentTo(command.Changes);
    }

    [TestCase("2025-08-20", Description = "Stale: a newer start date has since been saved")]
    [TestCase("not-a-date", Description = "Unparseable")]
    [TestCase("2025-08-01T00:00:00", Description = "Not ISO yyyy-MM-dd")]
    public async Task AndTheStartDateChangeDoesNotMatchThenNothingIsUpdated(string newStartDate)
    {
        var command = CreateCommand(newStartDate);

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task AndAnyChangeDoesNotMatchThenNothingIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Changes.Add(new ApprovedChange("StartDate", "2025-07-01", "2025-08-20"));

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [TestCase("Firstname")]
    [TestCase("Prices")]
    [TestCase("startDate", Description = "Wrong case is not recognised")]
    public async Task AndTheChangeTypeIsNotRecognisedThenTheFlagIsClearedAndTheLearningIsUpdated(string changeType)
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Changes.Add(new ApprovedChange(changeType, "Old", "New"));

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learningService.Verify(x => x.UpdateLearning(_learning), Times.Once);
    }

    [Test]
    public async Task AndTheChangeTypeIsNotRecognisedThenAWarningIsLogged()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Changes.Add(new ApprovedChange("Firstname", "Old", "New"));

        await _handler.Handle(command);

        _logger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Unrecognised change type Firstname")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Test]
    public async Task AndTheChangeTypeIsNotRecognisedAndTheStartDateChangeDoesNotMatchThenNothingIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-20");
        command.Changes.Insert(0, new ApprovedChange("Firstname", "Old", "New"));

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task AndTheStartDateChangeMatchesAndThePricesMatchThenTheFlagIsClearedAndTheLearningIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Prices = MatchingPrices();

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learningService.Verify(x => x.UpdateLearning(_learning), Times.Once);
    }

    [Test]
    public async Task AndTheStartDateChangeDoesNotMatchAndThePricesMatchThenNothingIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-20");
        command.Prices = MatchingPrices();

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task AndThePricesMatchThenTheRaisedEventCarriesTheApprovedPrices()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Prices = MatchingPrices();

        await _handler.Handle(command);

        _learning.FlushEvents().OfType<EmployerApprovalAppliedEvent>().Single()
            .Prices.Should().Equal(command.Prices);
    }

    [Test]
    public async Task AndOnlyPricesAreApprovedAndTheyMatchThenTheFlagIsClearedAndTheLearningIsUpdated()
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Changes = [];
        command.Prices = MatchingPrices();

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learningService.Verify(x => x.UpdateLearning(_learning), Times.Once);
    }

    [Test]
    public async Task AndTheStoredEpaPriceIsUnknownAndTheApprovedEpaPriceIsZeroThenTheFlagIsCleared()
    {
        _episode.Prices.Last().EndPointAssessmentPrice = null;
        _learning = CreateLearning(_episode);
        _learningService.Setup(x => x.GetLearning(_learning.Key, LearningType.Apprenticeship)).ReturnsAsync(_learning);
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Prices = MatchingPrices();
        command.Prices[1] = command.Prices[1] with { AssessmentPrice = 0 };

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
    }

    [TestCase("training", Description = "A training price differs")]
    [TestCase("epa", Description = "An EPA price differs")]
    [TestCase("epaUnknown", Description = "An approved EPA price is unknown where ours is not zero")]
    [TestCase("date", Description = "A later price's date differs")]
    [TestCase("extra", Description = "An extra price was approved")]
    [TestCase("missing", Description = "A price was not approved")]
    public async Task AndThePricesDoNotMatchThenNothingIsUpdated(string difference)
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        var prices = MatchingPrices();
        switch (difference)
        {
            case "training": prices[1] = prices[1] with { TrainingPrice = 1600 }; break;
            case "epa": prices[1] = prices[1] with { AssessmentPrice = 400 }; break;
            case "epaUnknown": prices[1] = prices[1] with { AssessmentPrice = null }; break;
            case "date": prices[1] = prices[1] with { EffectiveFrom = new DateTime(2026, 3, 1) }; break;
            case "extra": prices.Add(new ApprovedPrice(2000, 300, new DateTime(2026, 5, 1))); break;
            case "missing": prices.RemoveAt(1); break;
        }
        command.Prices = prices;

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [TestCase("Firstname")]
    [TestCase("Prices", Description = "Approvals' old shape: prices as a change item")]
    [TestCase(null, Description = "No changes at all")]
    public async Task AndNothingRecognisedIsApprovedThenNothingIsUpdated(string? changeType)
    {
        var command = CreateCommand(newStartDate: "2025-08-01");
        command.Changes = changeType == null ? [] : [new ApprovedChange(changeType, "Old", "New")];

        await _handler.Handle(command);

        _episode.FurtherApprovalNeeded.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task AndTheFlagIsAlreadyClearThenNothingIsUpdated()
    {
        _episode.FurtherApprovalNeeded = false;
        var command = CreateCommand(newStartDate: "2025-08-01");

        await _handler.Handle(command);

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
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

    private static List<ApprovedPrice> MatchingPrices() =>
    [
        new(1000, 200, StartDate),
        new(1500, 300, LaterPriceDate)
    ];

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
                EndDate = LaterPriceDate.AddDays(-1),
                TrainingPrice = 1000,
                EndPointAssessmentPrice = 200,
                TotalPrice = 1200
            },
            new()
            {
                Key = Guid.NewGuid(),
                EpisodeKey = episode.Key,
                StartDate = LaterPriceDate,
                EndDate = new DateTime(2026, 7, 31),
                TrainingPrice = 1500,
                EndPointAssessmentPrice = 300,
                TotalPrice = 1800
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
