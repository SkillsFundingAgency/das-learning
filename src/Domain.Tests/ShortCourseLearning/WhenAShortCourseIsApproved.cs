using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Domain.UnitTests.ShortCourseLearning;

[TestFixture]
public class WhenAShortCourseIsApproved
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndApprovedViaTheBaseLearningDomainModelType_ThenTheEpisodeIsApprovedAndAnEventIsRaised()
    {
        //Arrange
        var ukprn = _fixture.Create<long>();
        var learning = CreateLearning(ukprn);
        var episodeKeyBeforeApproval = learning.LatestEpisodeForProvider(ukprn).Key;
        LearningDomainModel baseLearning = learning;

        var context = new ApproveLearningContext
        {
            Ukprn = ukprn,
            EmployerAccountId = _fixture.Create<long>(),
            EmployerType = EmployerType.NonLevy,
            ApprovalsApprenticeshipId = _fixture.Create<long>(),
            TransferSenderId = _fixture.Create<long?>()
        };

        //Act
        baseLearning.Approve(context);

        //Assert
        var episode = learning.LatestEpisodeForProvider(ukprn);
        episode.IsApproved.Should().BeTrue();
        episode.EmployerAccountId.Should().Be(context.EmployerAccountId);
        episode.EmployerType.Should().Be(context.EmployerType);
        episode.ApprovalsApprenticeshipId.Should().Be(context.ApprovalsApprenticeshipId);
        episode.TransferSenderId.Should().Be(context.TransferSenderId);

        var events = learning.FlushEvents().OfType<LearningApprovedEvent>().ToList();
        events.Should().ContainSingle();
        var approvedEvent = events.Single();
        approvedEvent.LearningKey.Should().Be(learning.Key);
        approvedEvent.EpisodeKey.Should().Be(episodeKeyBeforeApproval);
        approvedEvent.ApprovalsApprenticeshipId.Should().Be(context.ApprovalsApprenticeshipId);
        approvedEvent.EmployerAccountId.Should().Be(context.EmployerAccountId);
        approvedEvent.FundingAccountId.Should().Be(context.TransferSenderId!.Value);
        approvedEvent.LearnerKey.Should().Be(learning.LearnerKey);
        approvedEvent.LearnerRef.Should().Be(episode.LearnerRef);
        approvedEvent.EmployerType.Should().Be(context.EmployerType);
    }

    private ShortCourseLearningDomainModel CreateLearning(long ukprn)
    {
        var episode = new DataAccess.Entities.Learning.ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            IsApproved = false,
            StartDate = new DateTime(2024, 1, 1),
            ExpectedEndDate = new DateTime(2024, 6, 1),
            ApprovalsApprenticeshipId = 0,
            EmployerAccountId = _fixture.Create<long>(),
            Ukprn = ukprn,
            TrainingCode = "CODE",
            LearnerRef = "LEARNER1",
            EmployerType = EmployerType.NonLevy
        };

        var entity = new DataAccess.Entities.Learning.ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = Guid.NewGuid(),
            Price = 1000,
            LearningType = LearningType.Apprenticeship,
            Episodes = new List<DataAccess.Entities.Learning.ShortCourseEpisode> { episode }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }
}
