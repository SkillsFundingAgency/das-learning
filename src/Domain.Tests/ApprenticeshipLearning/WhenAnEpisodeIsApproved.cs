using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenAnEpisodeIsApproved
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public void ThenTheEpisodeIsApprovedAndEmployerDetailsAreSet()
    {
        //Arrange
        var episode = ApprenticeshipEpisodeDomainModel.Get(_fixture.Build<ApprenticeshipEpisode>()
            .With(x => x.IsApproved, false)
            .With(x => x.LegalEntityName, string.Empty)
            .With(x => x.ApprovalsApprenticeshipId, 0)
            .Create());

        var employerAccountId = _fixture.Create<long>();
        var employerType = EmployerType.NonLevy;
        var fundingEmployerAccountId = _fixture.Create<long?>();
        var legalEntityName = _fixture.Create<string>();
        var approvalsApprenticeshipId = _fixture.Create<long>();

        //Act
        episode.Approve(employerAccountId, employerType, fundingEmployerAccountId, legalEntityName, approvalsApprenticeshipId);

        //Assert
        episode.IsApproved.Should().BeTrue();
        episode.EmployerAccountId.Should().Be(employerAccountId);
        episode.EmployerType.Should().Be(employerType);
        episode.FundingEmployerAccountId.Should().Be(fundingEmployerAccountId);
        episode.LegalEntityName.Should().Be(legalEntityName);
        episode.ApprovalsApprenticeshipId.Should().Be(approvalsApprenticeshipId);
    }
}
