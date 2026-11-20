using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests;

[TestFixture]
public class ApprenticeshipLearningSnapshotTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void FromMapsEpisodeForceEarningsSync(bool forceEarningsSync)
    {
        var fixture = new Fixture();
        var entity = fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        episode.ForceEarningsSync = forceEarningsSync;
        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        var learning = ApprenticeshipLearningDomainModel.Get(entity);

        var snapshot = ApprenticeshipLearningSnapshot.From(learning);

        snapshot.Episodes.Single().ForceEarningsSync.Should().Be(forceEarningsSync);
    }

    [TestCase(FundingPlatform.DAS)]
    [TestCase(FundingPlatform.SLD)]
    public void FromMapsEpisodeFundingPlatform(FundingPlatform fundingPlatform)
    {
        var fixture = new Fixture();
        var entity = fixture.Create<DataAccess.Entities.Learning.ApprenticeshipLearning>();
        var episode = fixture.Create<DataAccess.Entities.Learning.ApprenticeshipEpisode>();
        episode.FundingPlatform = fundingPlatform;
        entity.Episodes = new List<DataAccess.Entities.Learning.ApprenticeshipEpisode> { episode };
        var learning = ApprenticeshipLearningDomainModel.Get(entity);

        var snapshot = ApprenticeshipLearningSnapshot.From(learning);

        snapshot.Episodes.Single().FundingPlatform.Should().Be(fundingPlatform);
    }
}
