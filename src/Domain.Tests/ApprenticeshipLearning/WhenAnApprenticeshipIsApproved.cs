using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenAnApprenticeshipIsApproved
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenTheLatestEpisodeIsApprovedAndALearningApprovedEventIsRaised()
    {
        //Arrange
        var (learning, _) = new LearningDomainModelBuilder().Build();
        var episodeKeyBeforeApproval = learning.LatestEpisode.Key;

        var employerAccountId = _fixture.Create<long>();
        var employerType = EmployerType.NonLevy;
        var fundingEmployerAccountId = _fixture.Create<long?>();
        var legalEntityName = _fixture.Create<string>();
        var approvalsApprenticeshipId = _fixture.Create<long>();
        var accountLegalEntityId = _fixture.Create<long?>();
        var trainingCourseVersion = _fixture.Create<string>();

        //Act
        learning.Approve(employerAccountId, employerType, fundingEmployerAccountId, legalEntityName, approvalsApprenticeshipId, accountLegalEntityId, trainingCourseVersion);

        //Assert
        learning.LatestEpisode.IsApproved.Should().BeTrue();
        learning.LatestEpisode.EmployerAccountId.Should().Be(employerAccountId);
        learning.LatestEpisode.EmployerType.Should().Be(employerType);
        learning.LatestEpisode.FundingEmployerAccountId.Should().Be(fundingEmployerAccountId);
        learning.LatestEpisode.LegalEntityName.Should().Be(legalEntityName);
        learning.LatestEpisode.ApprovalsApprenticeshipId.Should().Be(approvalsApprenticeshipId);
        learning.LatestEpisode.AccountLegalEntityId.Should().Be(accountLegalEntityId);
        learning.TrainingCourseVersion.Should().Be(trainingCourseVersion);

        var events = learning.FlushEvents();
        events.Should().ContainSingle().Which.Should().BeOfType<LearningApprovedEvent>();
        var approvedEvent = (LearningApprovedEvent)events.Single();
        approvedEvent.LearningKey.Should().Be(learning.Key);
        approvedEvent.EpisodeKey.Should().Be(episodeKeyBeforeApproval);
        approvedEvent.ApprovalsApprenticeshipId.Should().Be(approvalsApprenticeshipId);
        approvedEvent.EmployerAccountId.Should().Be(employerAccountId);
        approvedEvent.FundingAccountId.Should().Be(fundingEmployerAccountId!.Value);
        approvedEvent.LearnerKey.Should().Be(learning.LearnerKey);
        approvedEvent.EmployerType.Should().Be(employerType);
    }

    [Test]
    public void AndNoFundingEmployerAccountIdIsProvided_ThenFundingAccountIdFallsBackToEmployerAccountId()
    {
        //Arrange
        var (learning, _) = new LearningDomainModelBuilder().Build();

        var employerAccountId = _fixture.Create<long>();

        //Act
        learning.Approve(employerAccountId, EmployerType.NonLevy, null, _fixture.Create<string>(), _fixture.Create<long>());

        //Assert
        var approvedEvent = (LearningApprovedEvent)learning.FlushEvents().Single();
        approvedEvent.FundingAccountId.Should().Be(employerAccountId);
    }

    [Test]
    public void AndApprovedViaTheBaseLearningDomainModelType_ThenItIsStillApprovedCorrectly()
    {
        //Arrange
        var (learning, _) = new LearningDomainModelBuilder().Build();
        LearningDomainModel baseLearning = learning;

        var context = new ApproveLearningContext
        {
            Ukprn = _fixture.Create<long>(),
            EmployerAccountId = _fixture.Create<long>(),
            EmployerType = EmployerType.NonLevy,
            ApprovalsApprenticeshipId = _fixture.Create<long>(),
            TransferSenderId = _fixture.Create<long?>(),
            LegalEntityName = _fixture.Create<string>(),
            AccountLegalEntityId = _fixture.Create<long?>(),
            TrainingCourseVersion = _fixture.Create<string>()
        };

        //Act
        baseLearning.Approve(context);

        //Assert
        learning.LatestEpisode.IsApproved.Should().BeTrue();
        learning.LatestEpisode.EmployerAccountId.Should().Be(context.EmployerAccountId);
        learning.LatestEpisode.EmployerType.Should().Be(context.EmployerType);
        learning.LatestEpisode.FundingEmployerAccountId.Should().Be(context.TransferSenderId);
        learning.LatestEpisode.LegalEntityName.Should().Be(context.LegalEntityName);
        learning.LatestEpisode.ApprovalsApprenticeshipId.Should().Be(context.ApprovalsApprenticeshipId);
        learning.LatestEpisode.AccountLegalEntityId.Should().Be(context.AccountLegalEntityId);
        learning.TrainingCourseVersion.Should().Be(context.TrainingCourseVersion);
    }
}
