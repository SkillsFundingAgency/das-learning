using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenCheckingIfApprovedAndPaidByDas
{
    [TestCase(true, FundingPlatform.DAS, true)]
    [TestCase(true, FundingPlatform.SLD, false)]
    [TestCase(false, FundingPlatform.DAS, false)]
    [TestCase(false, FundingPlatform.SLD, false)]
    public void ThenItIsTrueOnlyForAnApprovedEpisodePaidByDas(bool isApproved, FundingPlatform fundingPlatform, bool expected)
    {
        var entity = new Fixture().Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        entity.IsApproved = isApproved;
        entity.FundingPlatform = fundingPlatform;

        var episode = ApprenticeshipEpisodeDomainModel.Get(entity);

        episode.IsApprovedAndPaidByDas.Should().Be(expected);
    }
}
