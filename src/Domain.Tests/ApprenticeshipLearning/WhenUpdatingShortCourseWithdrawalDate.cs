using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenUpdatingShortCourseWithdrawalDate
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndWithdrawalDateSetOnApprovedEpisode_ThenLearningWithdrawnEventRaised()
    {
        var startDate = new DateTime(2024, 1, 1);
        var withdrawalDate = new DateTime(2024, 6, 1);
        var learning = CreateLearning(isApproved: true, startDate: startDate, withdrawalDate: null);
        var updateContext = CreateUpdateContext(learning, withdrawalDate);

        learning.Update(updateContext);

        var events = learning.FlushEvents().OfType<LearningWithdrawnEvent>().ToList();
        events.Should().ContainSingle();
        var @event = events.Single();
        @event.LearningKey.Should().Be(learning.Key);
        @event.ApprovalsApprenticeshipId.Should().Be(learning.LatestEpisode.ApprovalsApprenticeshipId);
        @event.EmployerAccountId.Should().Be(learning.LatestEpisode.EmployerAccountId);
        @event.LastDayOfLearning.Should().Be(withdrawalDate);
    }

    [Test]
    public void AndWithdrawalDateEqualsStartDate_ThenReasonIsStillWithdrawDuringLearning()
    {
        var startDate = new DateTime(2024, 1, 1);
        var learning = CreateLearning(isApproved: true, startDate: startDate, withdrawalDate: null);
        var updateContext = CreateUpdateContext(learning, withdrawalDate: startDate);

        learning.Update(updateContext);

        learning.FlushEvents().OfType<LearningWithdrawnEvent>().Should().ContainSingle();
    }

    [Test]
    public void AndEpisodeIsNotApproved_ThenNoEventRaised()
    {
        var learning = CreateLearning(isApproved: false, startDate: new DateTime(2024, 1, 1), withdrawalDate: null);
        var updateContext = CreateUpdateContext(learning, withdrawalDate: new DateTime(2024, 6, 1));

        learning.Update(updateContext);

        learning.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndWithdrawalDateCleared_ThenNoEventRaised()
    {
        var learning = CreateLearning(isApproved: true, startDate: new DateTime(2024, 1, 1), withdrawalDate: new DateTime(2024, 6, 1));
        var updateContext = CreateUpdateContext(learning, withdrawalDate: null);

        learning.Update(updateContext);

        learning.FlushEvents().OfType<LearningWithdrawnEvent>().Should().BeEmpty();
    }

    private ShortCourseLearningDomainModel CreateLearning(bool isApproved, DateTime startDate, DateTime? withdrawalDate)
    {
        var episode = new DataAccess.Entities.Learning.ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            IsApproved = isApproved,
            StartDate = startDate,
            ExpectedEndDate = startDate.AddMonths(6),
            WithdrawalDate = withdrawalDate,
            ApprovalsApprenticeshipId = isApproved ? _fixture.Create<long>() : 0,
            EmployerAccountId = _fixture.Create<long>(),
            Ukprn = _fixture.Create<long>(),
            TrainingCode = "CODE",
            LearnerRef = "LEARNER1",
            LearningType = LearningType.Apprenticeship,
            EmployerType = EmployerType.NonLevy
        };

        var entity = new DataAccess.Entities.Learning.ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = Guid.NewGuid(),
            Episodes = new List<DataAccess.Entities.Learning.ShortCourseEpisode> { episode }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }

    private static ShortCourseUpdateContext CreateUpdateContext(ShortCourseLearningDomainModel learning, DateTime? withdrawalDate)
    {
        var episode = learning.LatestEpisode;
        return new ShortCourseUpdateContext
        {
            LearnerRef = episode.LearnerRef,
            OnProgramme = new OnProgramme
            {
                Ukprn = episode.Ukprn,
                EmployerId = episode.EmployerAccountId,
                CourseCode = episode.TrainingCode,
                StartDate = episode.StartDate,
                ExpectedEndDate = episode.ExpectedEndDate,
                WithdrawalDate = withdrawalDate,
                Price = episode.Price,
                LearningType = episode.LearningType,
                Milestones = new List<Milestone>()
            },
            LearningSupport = new List<LearningSupportDetails>()
        };
    }
}
