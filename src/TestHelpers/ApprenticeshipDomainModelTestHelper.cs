using AutoFixture;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Types;
using System.Reflection;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;
using FundingType = SFA.DAS.Learning.Enums.FundingType;

namespace SFA.DAS.Learning.TestHelpers;

public static class ApprenticeshipDomainModelTestHelper
{
    private static readonly Fixture _fixture = new();

    // If this method isn't a sign that we need to refactor this project then I don't know what is
    public static ApprenticeshipLearningDomainModel CreateBasicTestModel()
    {
        // Create an instance with default constructor or Activator
        var apprenticeship = (ApprenticeshipLearningDomainModel)Activator.CreateInstance(
            typeof(ApprenticeshipLearningDomainModel),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { new DataAccess.Entities.Learning.ApprenticeshipLearning() },
            null
        )!;

        // Set private fields to empty lists using reflection
        typeof(ApprenticeshipLearningDomainModel)
            .GetField("_episodes", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(apprenticeship, new List<ApprenticeshipEpisodeDomainModel>());


        return apprenticeship;
    }

    public static void AddEpisode(ApprenticeshipLearningDomainModel learning, DateTime? startDate = null, DateTime? endDate = null, long? ukprn = null, FundingPlatform? fundingPlatform = FundingPlatform.DAS)
    {
        var start = startDate ?? _fixture.Create<DateTime>();
        var end = endDate ?? (start.AddDays(_fixture.Create<int>()));
        var ukprnValue = ukprn ?? _fixture.Create<long>();

        learning.AddEpisode(
            ukprnValue,
            _fixture.Create<long>(),
            start,
            end,
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<decimal>(),
            _fixture.Create<FundingType>(),
            fundingPlatform,
            _fixture.Create<long?>(),
            _fixture.Create<string>(),
            _fixture.Create<long>(),
            _fixture.Create<int>().ToString(),
            _fixture.Create<string?>());
    }

    public static bool DoEpisodeDetailsMatchDomainModel(LearningEvent e, ApprenticeshipLearningDomainModel learning, LearnerDomainModel learnerDomainModel)
    {
        var episode = learning.LatestEpisode;
        var expectedNumberOfPrices = learning.AllPrices.Count();
        var episodePrice = learning.LatestPrice;
        return
            e.Episode.TrainingCode == episode.TrainingCode &&
            e.Episode.FundingEmployerAccountId == episode.FundingEmployerAccountId &&
            e.Episode.EmployerAccountId == episode.EmployerAccountId &&
            e.Episode.LegalEntityName == episode.LegalEntityName &&
            e.Episode.Ukprn == episode.Ukprn &&
            e.Episode.AgeAtStartOfLearning == learning.AgeAtStartOfLearning(learnerDomainModel.ToModel()) &&
            e.Episode.Prices.Count == expectedNumberOfPrices &&
            e.Episode.Prices.MaxBy(x => x.StartDate)!.TotalPrice == episodePrice.TotalPrice &&
            e.Episode.FundingType == episode.FundingType &&
            e.Episode.Prices.MaxBy(x => x.StartDate)!.StartDate == episodePrice.StartDate &&
            e.Episode.Prices.MaxBy(x => x.StartDate)!.EndDate == episodePrice.EndDate &&
            e.Episode.FundingPlatform == episode.FundingPlatform;
    }
}
