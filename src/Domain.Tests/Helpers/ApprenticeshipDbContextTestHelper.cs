using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Helpers;

public static class ApprenticeshipDbContextTestHelper
{
    private static readonly Fixture _fixture = new();

    public static LearningQueryRepository SetUpApprenticeshipQueryRepository(this LearningDataContext dbContext)
    {
        dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
        var logger = Mock.Of<ILogger<LearningQueryRepository>>();
        return new LearningQueryRepository(new Lazy<LearningDataContext>(dbContext), logger);
    }

    public static async Task<DataAccess.Entities.Learning.Learning> AddApprenticeship(
        this LearningDataContext dbContext, 
        Guid apprenticeshipKey, 
        bool addPendingPriceHistoryRequest,
        long? ukprn = null,
        string? initiator = null,
        long? approvalsApprenticeshipId = null,
        FundingPlatform? fundingPlatform = null,
        DateTime? startDate = null,
        LearnerStatus? learnerStatus = null,
        bool addWithdrawalRequest = false,
        DateTime? endDate = null)
    {
        var episodeKey = _fixture.Create<Guid>();
        var episodePrice = _fixture.Build<EpisodePrice>()
            .With(x => x.EpisodeKey, episodeKey)
            .With(x => x.StartDate, startDate ?? _fixture.Create<DateTime>())
            .With(x => x.EndDate, endDate ?? _fixture.Create<DateTime>())
            .With(x => x.IsDeleted, false)
            .Create();

        var episode = _fixture.Build<Episode>()
            .With(x => x.LearningKey, apprenticeshipKey)
            .With(x => x.Key, episodeKey)
            .With(x => x.Ukprn, ukprn ?? _fixture.Create<long>())
            .With(x => x.FundingPlatform, fundingPlatform ?? _fixture.Create<FundingPlatform>())
            .With(x => x.Prices, new List<EpisodePrice> { episodePrice })
            .With(x => x.LearningStatus, learnerStatus != null ? learnerStatus.ToString() : _fixture.Create<LearnerStatus>().ToString())
            .Create();

        var apprenticeship = _fixture.Build<DataAccess.Entities.Learning.Learning>()
            .With(x => x.Key, apprenticeshipKey)
            .With(x => x.ApprovalsApprenticeshipId, approvalsApprenticeshipId ?? _fixture.Create<long>())
            .With(x => x.Episodes, new List<Episode>() { episode })
            .Create();

        if (addPendingPriceHistoryRequest)
        {
            var providerApprovedDate = initiator == "Provider" ? _fixture.Create<DateTime>() : (DateTime?)null;
            var employerApprovedDate = initiator == "Employer" ? _fixture.Create<DateTime>() : (DateTime?)null;

            apprenticeship.PriceHistories = new List<PriceHistory>()
            {
                new()
                {
                    Key = _fixture.Create<Guid>(),
                    LearningKey = apprenticeshipKey,
                    PriceChangeRequestStatus = ChangeRequestStatus.Created,
                    TrainingPrice = 10000,
                    AssessmentPrice = 3000,
                    TotalPrice = 13000,
                    EffectiveFromDate = _fixture.Create<DateTime>(),
                    ChangeReason = "testReason",
                    ProviderApprovedDate = providerApprovedDate,
                    EmployerApprovedDate = employerApprovedDate,
                    ProviderApprovedBy = initiator == "Provider" ? "Mr Provider" : null,
                    EmployerApprovedBy = initiator == "Employer" ? "Mr Employer" : null,
                    Initiator = initiator == "Employer" ? ChangeInitiator.Employer : ChangeInitiator.Provider
                }
            };
        }

        if (addWithdrawalRequest)
        {
            apprenticeship.WithdrawalRequests = new List<WithdrawalRequest>()
            {
                new()
                {
                    LearningKey = apprenticeshipKey,
                    CreatedDate = DateTime.UtcNow,
                    EpisodeKey = episodeKey,
                    Key = Guid.NewGuid(),
                    LastDayOfLearning = DateTime.UtcNow.AddDays(-1),
                    ProviderApprovedBy = _fixture.Create<string>(),
                    Reason = "WithdrawDuringLearning"
                }
            };
        }

        await dbContext.AddAsync(apprenticeship);
        await dbContext.SaveChangesAsync();
        return apprenticeship;
    }
}