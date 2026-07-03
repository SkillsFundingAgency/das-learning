using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;

namespace SFA.DAS.Learning.Domain.UnitTests.ShortCourseLearning;

public class WhenCheckingAcademicYearOverlap
{
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenContinuingEpisodeStartedWithinAYOverlaps()
    {
        var episode = BuildEpisode(startDate: new DateTime(2025, 9, 1), expectedEndDate: new DateTime(2026, 6, 30));
        episode.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenContinuingEpisodeStartedInPriorAYButWithNoActualEndDateOverlaps()
    {
        var episode = BuildEpisode(startDate: new DateTime(2024, 9, 1), expectedEndDate: new DateTime(2026, 6, 30));
        episode.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenEpisodeCompletedBeforeAYStartDoesNotOverlap()
    {
        var episode = BuildEpisode(startDate: new DateTime(2024, 9, 1), expectedEndDate: new DateTime(2025, 6, 30),
            completionDate: new DateTime(2025, 6, 1));
        episode.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    [Test]
    public void ThenEpisodeWithdrawnBeforeAYStartDoesNotOverlap()
    {
        var episode = BuildEpisode(startDate: new DateTime(2024, 9, 1), expectedEndDate: new DateTime(2025, 6, 30),
            withdrawalDate: new DateTime(2025, 6, 1));
        episode.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    [Test]
    public void ThenEpisodeCompletedDuringAYOverlaps()
    {
        var episode = BuildEpisode(startDate: new DateTime(2025, 9, 1), expectedEndDate: new DateTime(2026, 6, 30),
            completionDate: new DateTime(2026, 1, 1));
        episode.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenEpisodeWithdrawnDuringAYOverlaps()
    {
        var episode = BuildEpisode(startDate: new DateTime(2025, 9, 1), expectedEndDate: new DateTime(2026, 6, 30),
            withdrawalDate: new DateTime(2026, 1, 1));
        episode.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenEpisodeStartingAfterAYEndDoesNotOverlap()
    {
        var episode = BuildEpisode(startDate: new DateTime(2026, 9, 1), expectedEndDate: new DateTime(2027, 6, 30));
        episode.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    private ShortCourseEpisodeDomainModel BuildEpisode(
        DateTime startDate,
        DateTime expectedEndDate,
        DateTime? completionDate = null,
        DateTime? withdrawalDate = null)
    {
        var learning = ShortCourseLearningDomainModel.New(Guid.NewGuid(), _fixture.Create<string>());
        return learning.AddEpisode(
            ukprn: _fixture.Create<long>(),
            employerAccountId: _fixture.Create<long>(),
            learnerRef: _fixture.Create<string>(),
            trainingCode: _fixture.Create<string>(),
            isApproved: false,
            startDate: startDate,
            expectedEndDate: expectedEndDate,
            withdrawalDate: withdrawalDate,
            withdrawalReasonCode: null,
            milestones: [],
            completionDate: completionDate);
    }
}
