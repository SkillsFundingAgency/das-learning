using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenUpdatingShortCourseEpisode
{
    [Test]
    public void Update_Should_Update_All_Fields_When_Not_Approved()
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            Guid.NewGuid(),
            ukprn: 11111111,
            employerAccountId: 123,
            trainingCode: "OLD",
            learnerRef: "LEARNER1",
            isApproved: false,
            startDate: new DateTime(2024, 1, 1),
            expectedEndDate: new DateTime(2024, 6, 1),
            withdrawalDate: null);

        var updateContext = new ShortCourseUpdateContext
        {
            LearnerRef = "LEARNER2",
            OnProgramme = new OnProgramme
            {
                Ukprn = 99999999,
                EmployerId = 456,
                CourseCode = "NEW",
                StartDate = new DateTime(2025, 1, 1),
                ExpectedEndDate = new DateTime(2025, 12, 1),
                WithdrawalDate = new DateTime(2025, 6, 1),
                Price = 2000,
                LearningType = LearningType.ApprenticeshipUnit,
                Milestones = new List<Milestone>()
            },
            LearningSupport = new List<LearningSupportDetails>()
        };

        episode.Update(updateContext);

        episode.Ukprn.Should().Be(99999999);
        episode.EmployerAccountId.Should().Be(456);
        episode.TrainingCode.Should().Be("NEW");
        episode.StartDate.Should().Be(new DateTime(2025, 1, 1));
        episode.ExpectedEndDate.Should().Be(new DateTime(2025, 12, 1));
        episode.WithdrawalDate.Should().Be(new DateTime(2025, 6, 1));
        episode.Price.Should().Be(2000);
        episode.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
        episode.LearnerRef.Should().Be("LEARNER2");
    }

    [Test]
    public void Update_Should_Only_Update_Permitted_Fields_When_Approved()
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            Guid.NewGuid(),
            ukprn: 11111111,
            employerAccountId: 123,
            trainingCode: "OLD",
            learnerRef: "LEARNER1",
            isApproved: true,
            startDate: new DateTime(2024, 1, 1),
            expectedEndDate: new DateTime(2024, 6, 1),
            withdrawalDate: null,
            price: 1000,
            learningType: LearningType.Apprenticeship);

        var updateContext = new ShortCourseUpdateContext
        {
            LearnerRef = "LEARNER2",
            OnProgramme = new OnProgramme
            {
                Ukprn = 99999999,
                EmployerId = 456,
                CourseCode = "NEW",
                StartDate = new DateTime(2025, 1, 1),
                ExpectedEndDate = new DateTime(2025, 12, 1),
                WithdrawalDate = new DateTime(2025, 6, 1),
                Price = 2000,
                LearningType = LearningType.ApprenticeshipUnit,
                Milestones = new List<Milestone>()
            },
            LearningSupport = new List<LearningSupportDetails>()
        };

        episode.Update(updateContext);

        // Restricted — unchanged
        episode.Ukprn.Should().Be(11111111);
        episode.EmployerAccountId.Should().Be(123);
        episode.TrainingCode.Should().Be("OLD");
        episode.StartDate.Should().Be(new DateTime(2024, 1, 1));
        episode.ExpectedEndDate.Should().Be(new DateTime(2024, 6, 1));
        episode.Price.Should().Be(1000);
        episode.LearningType.Should().Be(LearningType.Apprenticeship);
        // Permitted — updated
        episode.WithdrawalDate.Should().Be(new DateTime(2025, 6, 1));
        episode.LearnerRef.Should().Be("LEARNER2");
    }

    [Test]
    public void Update_Should_Remove_Missing_Milestones()
    {
        // Arrange
        var episode = CreateEpisode();

        episode.AddMilestone(Milestone.ThirtyPercentLearningComplete);

        var updateContext = CreateUpdateContext(
            milestones: new List<Milestone> { Milestone.LearningComplete });

        // Act
        episode.Update(updateContext);

        // Assert
        episode.Milestones.Should().ContainSingle();
        episode.Milestones.First().Milestone.Should().Be(Milestone.LearningComplete);
    }

    [Test]
    public void Update_Should_Add_New_Milestones()
    {
        // Arrange
        var episode = CreateEpisode();

        episode.AddMilestone(Milestone.ThirtyPercentLearningComplete);

        var updateContext = CreateUpdateContext(
            milestones: new List<Milestone>
            {
                Milestone.ThirtyPercentLearningComplete,
                Milestone.LearningComplete
            });

        // Act
        episode.Update(updateContext);

        // Assert
        episode.Milestones.Should().HaveCount(2);
        episode.Milestones.Select(m => m.Milestone)
            .Should().BeEquivalentTo(new[]
            {
                Milestone.ThirtyPercentLearningComplete,
                Milestone.LearningComplete
            });
    }

    [Test]
    public void Update_Should_Remove_Obsolete_LearningSupport()
    {
        // Arrange
        var episode = CreateEpisode();

        episode.AddLearningSupport(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 1));

        episode.AddLearningSupport(
            new DateTime(2024, 3, 1),
            new DateTime(2024, 4, 1));

        var updateContext = CreateUpdateContext(
            milestones: new List<Milestone>(),
            learningSupport: new List<LearningSupportDetails>
            {
                new()
                {
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 2, 1)
                }
            });

        // Act
        episode.Update(updateContext);

        // Assert
        episode.LearningSupport.Should().HaveCount(1);
        episode.LearningSupport.First().StartDate.Should()
            .Be(new DateTime(2024, 1, 1));
    }

    [Test]
    public void Update_Should_Add_New_LearningSupport()
    {
        // Arrange
        var episode = CreateEpisode();
        episode.AddLearningSupport(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 1));

        var originalLearningSupportKey = episode.LearningSupport.First().Key;

        var updateContext = CreateUpdateContext(
            milestones: new List<Milestone>(),
            learningSupport: new List<LearningSupportDetails>
            {
                new()
                {
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 2, 1)
                },
                new()
                {
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 2, 1)
                }
            });

        // Act
        episode.Update(updateContext);

        // Assert
        episode.LearningSupport.Should().HaveCount(2);
        episode.LearningSupport.OrderBy(x=>x.StartDate).Last().StartDate
            .Should().Be(new DateTime(2025, 1, 1));
        episode.LearningSupport.Should().Contain(ls => ls.Key == originalLearningSupportKey);
    }

private static ShortCourseEpisodeDomainModel CreateEpisode(LearningType learningType = LearningType.Apprenticeship)
    {
        return ShortCourseEpisodeDomainModel.New(
            Guid.NewGuid(),
            11111111,
            123,
            "CODE",
            "LEARNER1",
            true,
            DateTime.Today,
            DateTime.Today.AddMonths(3),
            null,
            learningType: learningType);
    }

    private static ShortCourseUpdateContext CreateUpdateContext(
        List<Milestone> milestones,
        List<LearningSupportDetails>? learningSupport = null,
        LearningType learningType = LearningType.Apprenticeship)
    {
        return new ShortCourseUpdateContext
        {
            OnProgramme = new OnProgramme
            {
                Ukprn = 1,
                EmployerId = 1,
                CourseCode = "CODE",
                StartDate = DateTime.Today,
                ExpectedEndDate = DateTime.Today.AddMonths(3),
                WithdrawalDate = null,
                Milestones = milestones,
                LearningType = learningType
            },
            LearningSupport = learningSupport ?? new List<LearningSupportDetails>()
        };
    }
}