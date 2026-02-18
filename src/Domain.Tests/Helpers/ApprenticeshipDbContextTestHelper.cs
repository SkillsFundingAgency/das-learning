using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
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

    public static async Task<(DataAccess.Entities.Learning.ApprenticeshipLearning, DataAccess.Entities.Learning.Learner)> AddApprenticeship(
        this LearningDataContext dbContext, 
        Guid learningKey, 
        long? ukprn = null,
        string? initiator = null,
        long? approvalsApprenticeshipId = null,
        FundingPlatform? fundingPlatform = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DateTime? withdrawalDate = null)
    {
        var episodeKey = _fixture.Create<Guid>();
        var episodePrice = _fixture.Build<EpisodePrice>()
            .With(x => x.EpisodeKey, episodeKey)
            .With(x => x.StartDate, startDate ?? _fixture.Create<DateTime>())
            .With(x => x.EndDate, endDate ?? _fixture.Create<DateTime>())
            .Create();

        var episodeBreakInLearning = _fixture.Build<EpisodeBreakInLearning>()
            .With(x => x.EpisodeKey, episodeKey)
            .With(x => x.StartDate, _fixture.Create<DateTime>())
            .With(x => x.EndDate, _fixture.Create<DateTime>())
            .Create();

        var episode = _fixture.Build<ApprenticeshipEpisode>()
            .With(x => x.LearningKey, learningKey)
            .With(x => x.Key, episodeKey)
            .With(x => x.Ukprn, ukprn ?? _fixture.Create<long>())
            .With(x => x.FundingPlatform, fundingPlatform ?? _fixture.Create<FundingPlatform>())
            .With(x => x.Prices, new List<EpisodePrice> { episodePrice })
            .With(x => x.WithdrawalDate, withdrawalDate)
            .With(x => x.BreaksInLearning, new List<EpisodeBreakInLearning>{ episodeBreakInLearning })
            .Create();

        var apprenticeship = _fixture.Build<DataAccess.Entities.Learning.ApprenticeshipLearning>()
            .With(x => x.Key, learningKey)
            .With(x => x.ApprovalsApprenticeshipId, approvalsApprenticeshipId ?? _fixture.Create<long>())
            .With(x => x.Episodes, new List<ApprenticeshipEpisode>() { episode })
            .Create();

        await dbContext.AddAsync(apprenticeship);

        var learner = _fixture.Build<DataAccess.Entities.Learning.Learner>()
            .With(x => x.Key, apprenticeship.LearnerKey)
            .With(x => x.Uln, _fixture.Create<long>().ToString())
            .Create();
        await dbContext.AddAsync(learner);

        await dbContext.SaveChangesAsync();
        return new (apprenticeship, learner);
    }
}