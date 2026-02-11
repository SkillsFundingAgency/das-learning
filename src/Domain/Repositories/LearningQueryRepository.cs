using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningQueryRepository(Lazy<LearningDataContext> dbContext, ILogger<LearningQueryRepository> logger)
    : ILearningQueryRepository
{
    private LearningDataContext DbContext => dbContext.Value;

    public async Task<IEnumerable<Learning.DataTransferObjects.Learning>> GetAll(long ukprn, FundingPlatform? fundingPlatform)
    {
        var apprenticeships = await DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.Episodes)
            .Include(x => x.Learner)
            .Where(x => x.Episodes.Any(y => y.Ukprn == ukprn && (fundingPlatform == null || y.FundingPlatform == fundingPlatform)))
            .ToListAsync();

        var result = apprenticeships.Select(x => new Learning.DataTransferObjects.Learning { Uln = x.Learner.Uln, LastName = x.Learner.LastName, FirstName = x.Learner.FirstName });
        return result;
    }

    public async Task<PagedResult<Learning.DataTransferObjects.Learning>> GetByDates(long ukprn, DateRange dates, int limit, int offset, CancellationToken cancellationToken)
    {
        var query = DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.Learner)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn))
            .Where(x => x.Episodes.Any(e =>
                e.Prices.Any(p =>
                    (p.StartDate >= dates.Start && p.StartDate <= dates.End)    // Start date is within academic year
                    | (p.EndDate >= dates.Start && p.EndDate <= dates.End)      // End date is within academic year     
                    | (p.StartDate <= dates.End && p.EndDate >= dates.Start)    // Start date is before academic year and end date is after academic year
                    &&
                    ( !e.WithdrawalDate.HasValue ||
                    ( 
                        e.WithdrawalDate.Value >= dates.Start &&             // Last day of learning is after the start of academic year
                        e.WithdrawalDate != p.StartDate)                     // and last day of learning is not the same as start date
                    )
                ))
            )
            .OrderBy(x => x.ApprovalsApprenticeshipId)
            .AsNoTracking();

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);

        var result = await query
            .Skip(offset)
            .Take(limit)
            .Select(x => new Learning.DataTransferObjects.Learning
            {
                Uln = x.Learner.Uln,
                Key = x.Key
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<Learning.DataTransferObjects.Learning>
        {
            Data = result,
            TotalItems = totalItems,
            TotalPages = totalPages,
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
                .Include(x => x.Learner)
                .Include(x => x.Episodes)
                .ThenInclude(x => x.Prices)
                .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn && e.FundingPlatform == FundingPlatform.DAS))
                .Where(x => !activeOnDate.HasValue ||
                    x.Episodes.Any(episode =>
                        episode.Prices.Any(price => price.EndDate >= activeOnDate.Value.StartOfCurrentAcademicYear()) && // end date is at least after the start of this academic year
                        episode.Prices.Any(price => price.StartDate <= activeOnDate.Value) &&      // start date is at least before the requested date
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value == episode.Prices.Min(p => p.StartDate)) //not withdrawn back to start
                        ))
                .OrderBy(x => x.Learner.Uln)
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
                new LearningWithEpisodes(
                    apprenticeship.Key,
                    apprenticeship.Learner.Uln,
                    apprenticeship.GetStartDate(),
                    apprenticeship.GetPlannedEndDate(),
                    apprenticeship.Episodes.Select(ep =>
                            new Episode(ep.Key, ep.TrainingCode, ep.WithdrawalDate, ep.Prices.Select(p =>
                                new EpisodePrice(p.Key, p.StartDate, p.EndDate, p.TrainingPrice, p.EndPointAssessmentPrice, p.TotalPrice)).ToList()))
                        .ToList(),
                    apprenticeship.GetAgeAtStartOfApprenticeship(),
                    apprenticeship.GetWithdrawalDate(),
                    apprenticeship.CompletionDate)
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