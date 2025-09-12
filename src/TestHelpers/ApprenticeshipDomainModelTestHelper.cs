using System.Reflection;
using AutoFixture;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;
using FundingType = SFA.DAS.Learning.Enums.FundingType;

namespace SFA.DAS.Learning.TestHelpers;

public static class ApprenticeshipDomainModelTestHelper
{
    private static readonly Fixture _fixture = new();

    // If this method isn't a sign that we need to refactor this project then I don't know what is
    public static LearningDomainModel CreateBasicTestModel()
    {
        // Create an instance with default constructor or Activator
        var apprenticeship = (LearningDomainModel)Activator.CreateInstance(
            typeof(LearningDomainModel),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { new DataAccess.Entities.Learning.Learning() },
            null
        );

        // Set private fields to empty lists using reflection
        typeof(LearningDomainModel)
            .GetField("_episodes", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(apprenticeship, new List<EpisodeDomainModel>());

        typeof(LearningDomainModel)
            .GetField("_startDateChanges", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(apprenticeship, new List<StartDateChangeDomainModel>());

        typeof(LearningDomainModel)
            .GetField("_freezeRequests", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(apprenticeship, new List<FreezeRequestDomainModel>());

        return apprenticeship;
    }

    public static void AddEpisode(LearningDomainModel learning, DateTime? startDate = null, DateTime? endDate = null, long? ukprn = null, FundingPlatform? fundingPlatform = FundingPlatform.DAS)
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
            _fixture.Create<int>(),
            _fixture.Create<long?>(),
            _fixture.Create<string>(),
            _fixture.Create<long>(),
            _fixture.Create<int>().ToString(),
            _fixture.Create<string?>());
    }
    public static void AddPendingStartDateChange(LearningDomainModel learning, ChangeInitiator changeInitiator, DateTime? startDate = null)
    {
        var start = startDate ?? _fixture.Create<DateTime>();
        var end = start.AddDays(_fixture.Create<int>());
        learning.AddStartDateChange(
            start,
            end,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<DateTime>(),
            null,
            null,
            _fixture.Create<DateTime>(),
            ChangeRequestStatus.Created,
            changeInitiator);
    }

    public static LearningDomainModel BuildApprenticeshipWithPendingStartDateChange(
    bool pendingProviderApproval = false,
    DateTime? originalStartDate = null,
    DateTime? newStartDate = null,
    DateTime? originalEndDate = null,
    DateTime? newEndDate = null)
    {
        _fixture.Customize(new ApprenticeshipCustomization());
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, originalStartDate, originalEndDate);

        var startDateEntity = _fixture.Build<StartDateChange>().With(x => x.ActualStartDate, newStartDate ?? _fixture.Create<DateTime>()).Create();
        startDateEntity.PlannedEndDate = newEndDate ?? startDateEntity.ActualStartDate.AddMonths(24);
        var startDateChange = StartDateChangeDomainModel.Get(startDateEntity);

        if (pendingProviderApproval)
        {
            apprenticeship.AddStartDateChange(startDateChange.ActualStartDate, startDateChange.PlannedEndDate, startDateChange.Reason,
                null, null,
                startDateChange.EmployerApprovedBy, startDateChange.EmployerApprovedDate, startDateChange.CreatedDate,
                ChangeRequestStatus.Created, ChangeInitiator.Employer);
        }
        else
        {
            apprenticeship.AddStartDateChange(startDateChange.ActualStartDate, startDateChange.PlannedEndDate, startDateChange.Reason,
                startDateChange.ProviderApprovedBy, startDateChange.ProviderApprovedDate,
                null, null, startDateChange.CreatedDate,
                ChangeRequestStatus.Created, ChangeInitiator.Provider);
        }

        return apprenticeship;
    }

    public static bool DoEpisodeDetailsMatchDomainModel(LearningEvent e, LearningDomainModel learning)
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
            e.Episode.AgeAtStartOfLearning == learning.AgeAtStartOfApprenticeship &&
            e.Episode.Prices.Count == expectedNumberOfPrices &&
            e.Episode.Prices.MaxBy(x => x.StartDate).TotalPrice == episodePrice.TotalPrice &&
            e.Episode.FundingType == episode.FundingType &&
            e.Episode.Prices.MaxBy(x => x.StartDate).StartDate == episodePrice.StartDate &&
            e.Episode.Prices.MaxBy(x => x.StartDate).EndDate == episodePrice.EndDate &&
            e.Episode.FundingPlatform == episode.FundingPlatform;
    }
}
