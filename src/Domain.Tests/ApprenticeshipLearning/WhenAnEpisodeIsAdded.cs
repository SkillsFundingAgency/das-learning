using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenAnEpisodeIsAdded
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public void ThenAnEpisodeAndPriceIsAdded()
    {
        //Arrange
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var episodePrice = _fixture.Build<EpisodePrice>().Create();
        var episode = ApprenticeshipEpisodeDomainModel.Get(_fixture.Build<ApprenticeshipEpisode>()
            .With(x => x.Prices, new List<EpisodePrice>(){ episodePrice })
            .With(x => x.PaymentsFrozen, false)
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.PauseDate, (DateTime?)null)
            .Create());

        //Act
        apprenticeship.AddEpisode(
            episode.ApprovalsApprenticeshipId,
            episode.Ukprn,
            episode.EmployerAccountId,
            episodePrice.StartDate,
            episodePrice.EndDate,
            episodePrice.TotalPrice,
            episodePrice.TrainingPrice,
            episodePrice.EndPointAssessmentPrice,
            episode.FundingEmployerAccountId,
            episode.LegalEntityName,
            episode.AccountLegalEntityId,
            episode.EmployerType,
            episode.IsApproved);

        //Assert
        apprenticeship.LatestEpisode.Should().BeEquivalentTo(episode, x => x
            .Excluding(y => y.Key)
            .Excluding(y => y.LearningKey)
            .Excluding(y => y.LatestPrice)
            .Excluding(y => y.EpisodePrices)
            .Excluding(y => y.FirstPrice)
            .Excluding(y => y.ActiveEpisodePrices)
            .Excluding(y => y.LearningSupport)
            .Excluding(y => y.IsRemoved)
            .Excluding(y => y.CompletionDate)
            .Excluding(y => y.AchievementDate)
            .Excluding(y => y.EpisodeBreaksInLearning));
        apprenticeship.LatestEpisode.LatestPrice.Should().BeEquivalentTo(episode.LatestPrice, x => x
            .ExcludingNestedObjects()
            .Excluding(y => y.Key));
        apprenticeship.LatestEpisode.LearningKey.Should().Be(apprenticeship.Key);
    }

    [Test]
    public void ThenAnEpisodeWithMultipleCostsAddsMultiplePrices()
    {
        //Arrange
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var endDate = new DateTime(2026, 07, 31);
        var costs = new List<Cost>
        {
            new()
            {
                FromDate = new DateTime(2025, 08, 01),
                TrainingPrice = 1000,
                EpaoPrice = 200
            },
            new()
            {
                FromDate = new DateTime(2026, 01, 01),
                TrainingPrice = 1500,
                EpaoPrice = 300
            }
        };

        //Act
        apprenticeship.AddEpisode(
            _fixture.Create<long>(),
            _fixture.Create<long>(),
            _fixture.Create<long?>(),
            endDate,
            _fixture.Create<long?>(),
            _fixture.Create<string>(),
            _fixture.Create<long?>(),
            default,
            costs);

        //Assert
        var prices = apprenticeship.LatestEpisode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .ToList();

        prices.Should().HaveCount(2);
        prices[0].StartDate.Should().Be(new DateTime(2025, 08, 01));
        prices[0].TrainingPrice.Should().Be(1000);
        prices[0].EndPointAssessmentPrice.Should().Be(200);
        prices[0].EndDate.Should().Be(endDate);
        prices[1].StartDate.Should().Be(new DateTime(2026, 01, 01));
        prices[1].TrainingPrice.Should().Be(1500);
        prices[1].EndPointAssessmentPrice.Should().Be(300);
        prices[1].EndDate.Should().Be(endDate);
    }
}