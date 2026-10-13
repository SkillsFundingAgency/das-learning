using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenCreatingADraft
{
    private static readonly Guid LearnerKey = Guid.NewGuid();
    private const long Ukprn = 12345678;
    private const string TrainingCode = "ST0001";

    private static DraftApprenticeshipDetails CreateDetails(
        DateTime? withdrawalDate = null,
        DateTime? completionDate = null,
        DateTime? achievementDate = null,
        DateTime? pauseDate = null,
        List<BreakInLearningUpdateDetails>? breaks = null,
        List<LearningSupportDetails>? learningSupport = null,
        List<EnglishAndMathsUpdateDetails>? englishAndMaths = null)
        => new()
        {
            ApprovalsApprenticeshipId = 0,
            LearnerRef = "REF1",
            Cost = new Cost { FromDate = new DateTime(2025, 8, 1), TrainingPrice = 1000, EpaoPrice = 200 },
            ExpectedEndDate = new DateTime(2026, 7, 31),
            WithdrawalDate = withdrawalDate,
            CompletionDate = completionDate,
            AchievementDate = achievementDate,
            PauseDate = pauseDate,
            BreaksInLearning = breaks ?? [],
            LearningSupport = learningSupport ?? [],
            EnglishAndMathsCourses = englishAndMaths ?? []
        };

    private static ApprenticeshipLearningDomainModel Create(DraftApprenticeshipDetails details)
        => ApprenticeshipLearningDomainModel.CreateDraft(LearnerKey, Ukprn, TrainingCode, details);

    [Test]
    public void Then_The_Draft_Is_Unapproved_With_The_Given_Identity_And_Price()
    {
        var learning = Create(CreateDetails());

        learning.LearningType.Should().Be(LearningType.Apprenticeship);
        learning.TrainingCode.Should().Be(TrainingCode);
        var episode = learning.LatestEpisode;
        episode.IsApproved.Should().BeFalse();
        episode.Ukprn.Should().Be(Ukprn);
        episode.EmployerAccountId.Should().BeNull();
        episode.EmployerType.Should().Be(EmployerType.Levy);
        episode.LearnerRef.Should().Be("REF1");
        episode.EpisodePrices.Should().ContainSingle();
        episode.FirstPrice.StartDate.Should().Be(new DateTime(2025, 8, 1));
        episode.FirstPrice.EndDate.Should().Be(new DateTime(2026, 7, 31));
        episode.FirstPrice.TrainingPrice.Should().Be(1000);
        episode.FirstPrice.EndPointAssessmentPrice.Should().Be(200);
        episode.FirstPrice.TotalPrice.Should().Be(1200);
    }

    [Test]
    public void Then_The_Learning_Type_Is_Taken_From_The_Details()
    {
        var details = CreateDetails();
        var foundation = new DraftApprenticeshipDetails
        {
            Cost = details.Cost,
            ExpectedEndDate = details.ExpectedEndDate,
            LearningType = LearningType.FoundationApprenticeship
        };

        Create(foundation).LearningType.Should().Be(LearningType.FoundationApprenticeship);
    }

    [Test]
    public void Then_A_Draft_Created_With_A_WithdrawalDate_Is_Withdrawn_And_Raises_No_Events()
    {
        var withdrawalDate = new DateTime(2025, 10, 1);

        var learning = Create(CreateDetails(withdrawalDate: withdrawalDate));

        learning.LatestEpisode.WithdrawalDate.Should().Be(withdrawalDate);
        learning.FlushEvents().Should().BeEmpty();
    }

    [Test]
    public void Then_Completion_Achievement_And_Pause_Dates_Are_Set()
    {
        var learning = Create(CreateDetails(
            completionDate: new DateTime(2026, 6, 1),
            achievementDate: new DateTime(2026, 6, 15),
            pauseDate: new DateTime(2026, 1, 10)));

        learning.LatestEpisode.CompletionDate.Should().Be(new DateTime(2026, 6, 1));
        learning.LatestEpisode.AchievementDate.Should().Be(new DateTime(2026, 6, 15));
        learning.LatestEpisode.PauseDate.Should().Be(new DateTime(2026, 1, 10));
        learning.FlushEvents().OfType<EndDateChangedEvent>().Should().BeEmpty();
    }

    [Test]
    public void Then_Breaks_In_Learning_And_Learning_Support_Are_Set_On_The_Episode()
    {
        var learning = Create(CreateDetails(
            breaks: [new BreakInLearningUpdateDetails { StartDate = new DateTime(2025, 12, 1), EndDate = new DateTime(2025, 12, 31), PriorPeriodExpectedEndDate = new DateTime(2026, 7, 31) }],
            learningSupport: [new LearningSupportDetails { StartDate = new DateTime(2025, 8, 1), EndDate = new DateTime(2026, 7, 31) }]));

        learning.LatestEpisode.EpisodeBreaksInLearning.Should().ContainSingle(x => x.StartDate == new DateTime(2025, 12, 1));
        learning.LatestEpisode.LearningSupport.Should().ContainSingle(x => x.StartDate == new DateTime(2025, 8, 1));
    }

    [Test]
    public void Then_English_And_Maths_Courses_Are_Added_To_The_Learning()
    {
        var learning = Create(CreateDetails(englishAndMaths:
        [
            new EnglishAndMathsUpdateDetails
            {
                Course = "Maths",
                LearnAimRef = "MATH01",
                StartDate = new DateTime(2025, 8, 1),
                PlannedEndDate = new DateTime(2026, 7, 31),
                WithdrawalDate = new DateTime(2026, 1, 1),
                Amount = 100,
                BreaksInLearning = [],
                LearningSupport = []
            }
        ]));

        var course = learning.EnglishAndMathsCourses.Should().ContainSingle().Subject;
        course.LearnAimRef.Should().Be("MATH01");
        course.WithdrawalDate.Should().Be(new DateTime(2026, 1, 1));
        course.LearningKey.Should().Be(learning.Key);
    }
}
