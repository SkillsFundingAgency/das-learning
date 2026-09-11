using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

public class WhenCheckingAcademicYearOverlap
{
    [Test]
    public void ThenApprenticeshipStartingAndEndingWithinAYOverlaps()
    {
        var learning = BuildLearning(startDate: new DateTime(2025, 9, 1), endDate: new DateTime(2026, 6, 30));
        learning.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenContinuingApprenticeshipStartedInPriorAYWithNoCompletionOrWithdrawalOverlaps()
    {
        var learning = BuildLearning(startDate: new DateTime(2024, 9, 1), endDate: new DateTime(2026, 6, 30));
        learning.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenApprenticeshipCompletedBeforeAYStartDoesNotOverlap()
    {
        var learning = BuildLearning(startDate: new DateTime(2024, 9, 1), endDate: new DateTime(2025, 6, 30),
            completionDate: new DateTime(2025, 6, 1));
        learning.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    [Test]
    public void ThenApprenticeshipWithdrawnBeforeAYStartDoesNotOverlap()
    {
        var learning = BuildLearning(startDate: new DateTime(2024, 9, 1), endDate: new DateTime(2025, 6, 30),
            withdrawalDate: new DateTime(2025, 6, 1));
        learning.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    [Test]
    public void ThenApprenticeshipCompletedDuringAYOverlaps()
    {
        var learning = BuildLearning(startDate: new DateTime(2025, 9, 1), endDate: new DateTime(2026, 6, 30),
            completionDate: new DateTime(2026, 1, 1));
        learning.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenApprenticeshipWithdrawnDuringAYOverlaps()
    {
        var learning = BuildLearning(startDate: new DateTime(2025, 9, 1), endDate: new DateTime(2026, 6, 30),
            withdrawalDate: new DateTime(2026, 1, 1));
        learning.OverlapsAcademicYear(2526).Should().BeTrue();
    }

    [Test]
    public void ThenApprenticeshipStartingAfterAYEndDoesNotOverlap()
    {
        var learning = BuildLearning(startDate: new DateTime(2026, 9, 1), endDate: new DateTime(2027, 6, 30));
        learning.OverlapsAcademicYear(2526).Should().BeFalse();
    }

    private static ApprenticeshipLearningDomainModel BuildLearning(
        DateTime startDate,
        DateTime endDate,
        DateTime? completionDate = null,
        DateTime? withdrawalDate = null)
    {
        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = 10005077,
            TrainingCode = "21",
            WithdrawalDate = withdrawalDate,
            CompletionDate = completionDate,
            Prices = new List<EpisodePrice>
            {
                new()
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = 1000
                }
            }
        };

        var entity = new DataAccess.Entities.Learning.ApprenticeshipLearning
        {
            Key = Guid.NewGuid(),
            Episodes = new List<ApprenticeshipEpisode> { episode }
        };

        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
