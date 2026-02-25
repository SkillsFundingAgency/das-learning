using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
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
            episode.Ukprn,
            episode.EmployerAccountId,
            episodePrice.StartDate,
            episodePrice.EndDate,
            episodePrice.TotalPrice,
            episodePrice.TrainingPrice,
            episodePrice.EndPointAssessmentPrice,
            episode.FundingType,
            episode.FundingPlatform,
            episode.FundingEmployerAccountId,
            episode.LegalEntityName,
            episode.AccountLegalEntityId,
            episode.TrainingCode,
            episode.TrainingCourseVersion);

        //Assert
        apprenticeship.LatestEpisode.Should().BeEquivalentTo(episode, x => x
            .Excluding(y => y.Key)
            .Excluding(y => y.LatestPrice)
            .Excluding(y => y.EpisodePrices)
            .Excluding(y => y.FirstPrice)
            .Excluding(y => y.ActiveEpisodePrices)
            .Excluding(y => y.LearningSupport)
            .Excluding(y => y.EpisodeBreaksInLearning));
        apprenticeship.LatestEpisode.LatestPrice.Should().BeEquivalentTo(episode.LatestPrice, x => x
            .ExcludingNestedObjects()
            .Excluding(y => y.Key));
    }
}