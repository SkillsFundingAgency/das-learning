using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningQueryRepository(Lazy<LearningDataContext> dbContext, ILogger<LearningQueryRepository> logger)
    : ILearningQueryRepository
{
    private LearningDataContext DbContext => dbContext.Value;

    public async Task<IEnumerable<Models.Dtos.Learning>> GetAll(
        long ukprn,
        FundingPlatform? fundingPlatform)
    {
        return await DbContext.ApprenticeshipLearningDbSet
            .Where(al => al.Episodes.Any(e =>
                e.Ukprn == ukprn &&
                (!fundingPlatform.HasValue || e.FundingPlatform == fundingPlatform)))
            .Join(
                DbContext.LearnersDbSet,
                al => al.LearnerKey,
                learner => learner.Key,
                (al, learner) => new Models.Dtos.Learning
                {
                    Uln = learner.Uln,
                    FirstName = learner.FirstName,
                    LastName = learner.LastName
                })
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<PagedResult<Models.Dtos.Learning>> GetByDates(
        long ukprn,
        DateRange dates,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var baseQuery = DbContext.ApprenticeshipLearningDbSet
            .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn))
            .Where(x => x.Episodes.Any(e =>
                e.Prices.Any(p =>
                    (
                        // Standard date range overlap check
                        p.StartDate <= dates.End &&
                        p.EndDate >= dates.Start
                    )
                    &&
                    (
                        !e.WithdrawalDate.HasValue ||
                        (
                            e.WithdrawalDate.Value >= dates.Start &&
                            e.WithdrawalDate != p.StartDate
                        )
                    )
                )))
            .AsNoTracking();

        // Count query
        var totalItems = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);

        // Paged + joined query
        var result = await baseQuery
            .OrderBy(x => x.ApprovalsApprenticeshipId)
            .Skip(offset)
            .Take(limit)
            .Join(
                DbContext.LearnersDbSet.AsNoTracking(),
                learning => learning.LearnerKey,
                learner => learner.Key,
                (learning, learner) => new Models.Dtos.Learning
                {
                    Uln = learner.Uln,
                    Key = learning.Key
                })
            .ToListAsync(cancellationToken);

        return new PagedResult<Models.Dtos.Learning>
        {
            Data = result,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }


    public async Task<Guid?> GetKeyByLearningId(long learningId)
    {
        var apprenticeshipWithMatchingId = await DbContext.ApprenticeshipLearningDbSet
            .SingleOrDefaultAsync(x => x.ApprovalsApprenticeshipId == learningId);
        return apprenticeshipWithMatchingId?.Key;
    }

    /// <summary>
    /// Get learnings with episodes for a provider
    /// </summary>
    /// <param name="ukprn">The unique provider reference number. Only learnings where the episode with this provider reference will be returned.</param>
    /// <param name="activeOnDate">If populated, will return only learnings that are active on this date</param>
    /// <param name="limit">pagination limit</param>
    /// <param name="offset">pagination offset</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    public async Task<PagedResult<LearningWithEpisodes>?> GetLearningsWithEpisodes(long ukprn, DateTime? activeOnDate = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbContext.ApprenticeshipLearningDbSet
                .Include(x => x.Episodes)
                .ThenInclude(x => x.Prices)
                .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn && e.FundingPlatform == FundingPlatform.DAS))
                .Where(x => !activeOnDate.HasValue ||
                    x.Episodes.Any(episode =>
                        episode.Prices.Any(price => price.EndDate >= activeOnDate.Value.StartOfCurrentAcademicYear()) && // end date is at least after the start of this academic year
                        episode.Prices.Any(price => price.StartDate <= activeOnDate.Value) &&      // start date is at least before the requested date
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value == episode.Prices.Min(p => p.StartDate)) //not withdrawn back to start
                        ))
                .OrderBy(x => x.ApprovalsApprenticeshipId)
                .AsNoTracking();

            List<DataAccess.Entities.Learning.ApprenticeshipLearning> apprenticeships;

            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling((double)totalItems / limit.GetValueOrDefault());

            if (limit.HasValue && offset.HasValue)
            {
                apprenticeships = await query
                    .Skip(offset.Value)
                    .Take(limit.Value)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                apprenticeships = await query.ToListAsync(cancellationToken);
            }

            var apprenticeshipWithEpisodes = apprenticeships.Select(apprenticeship =>
            {
                var learner = DbContext.LearnersDbSet.Single(l => l.Key == apprenticeship.LearnerKey);

                return new LearningWithEpisodes(
                    apprenticeship.Key,
                    learner.Uln,
                    apprenticeship.GetStartDate(),
                    apprenticeship.GetPlannedEndDate(),
                    apprenticeship.Episodes.Select(ep =>
                            new Episode(ep.Key, ep.TrainingCode, ep.WithdrawalDate, ep.Prices.Select(p =>
                                new EpisodePrice(p.Key, p.StartDate, p.EndDate, p.TrainingPrice, p.EndPointAssessmentPrice, p.TotalPrice)).ToList()))
                        .ToList(),
                    apprenticeship.GetAgeAtStartOfApprenticeship(learner.DateOfBirth),
                    apprenticeship.GetWithdrawalDate(),
                    apprenticeship.CompletionDate);
            }

            ).ToList();

            return new PagedResult<LearningWithEpisodes>
            {
                Data = apprenticeshipWithEpisodes,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting apprenticeships with episodes for provider UKPRN {Ukprn}", ukprn);
            return null;
        }
    }

}