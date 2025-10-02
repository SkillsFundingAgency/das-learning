using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningQueryRepository(Lazy<LearningDataContext> dbContext, ILogger<LearningQueryRepository> logger)
    : ILearningQueryRepository
{
    private LearningDataContext DbContext => dbContext.Value;

    public async Task<IEnumerable<Learning.DataTransferObjects.Learning>> GetAll(long ukprn, FundingPlatform? fundingPlatform)
    {
        var apprenticeships = await DbContext.Apprenticeships
            .Include(x => x.Episodes)
            .Where(x => x.Episodes.Any(y => y.Ukprn == ukprn && (fundingPlatform == null || y.FundingPlatform == fundingPlatform)))
            .ToListAsync();

        var result = apprenticeships.Select(x => new Learning.DataTransferObjects.Learning { Uln = x.Uln, LastName = x.LastName, FirstName = x.FirstName });
        return result;
    }

    public async Task<PagedResult<Learning.DataTransferObjects.Learning>> GetByDates(long ukprn, DateRange dates, int limit, int offset, CancellationToken cancellationToken)
    {
        var query = DbContext.ApprenticeshipsDbSet
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn))
            .Where(x => x.Episodes.Any(e =>
                e.Prices.Any(p =>
                    (p.StartDate >= dates.Start && p.StartDate <= dates.End)
                    | (p.EndDate >= dates.Start && p.EndDate <= dates.End)
                    | (p.StartDate <= dates.End && p.EndDate >= dates.Start)
                )))
            .Where(x => x.Episodes.Any(e => e.LearningStatus == nameof(LearnerStatus.Active)))
            .OrderBy(x => x.ApprovalsApprenticeshipId)
            .AsNoTracking();

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);

        var result = await query
            .Skip(offset)
            .Take(limit)
            .Select(x => new Learning.DataTransferObjects.Learning
            {
                Uln = x.Uln,
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
        var apprenticeshipWithMatchingId = await DbContext.Apprenticeships
            .SingleOrDefaultAsync(x => x.ApprovalsApprenticeshipId == learningId);
        return apprenticeshipWithMatchingId?.Key;
    }

    public async Task<ApprenticeshipPrice?> GetPrice(Guid learningKey)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .FirstOrDefaultAsync(x => x.Key == learningKey);

        var episodes = apprenticeship?.Episodes.ToList();
        var prices = episodes?.SelectMany(x => x.Prices).ToList();

        var latestPrice = prices?.MaxBy(x => x.StartDate);
        if (latestPrice == null)
        {
            return null;
        }

        var latestEpisode = episodes?.MaxBy(x => x.Prices.Select(x => x.StartDate));
        if (latestEpisode == null)
        {
            return null;
        }

        var firstPrice = prices?.MinBy(x => x.StartDate);
        if (firstPrice == null)
        {
            return null;
        }

        return new ApprenticeshipPrice
        {
            TotalPrice = latestPrice.TotalPrice,
            AssessmentPrice = latestPrice.EndPointAssessmentPrice,
            TrainingPrice = latestPrice.TrainingPrice,
            FundingBandMaximum = latestPrice.FundingBandMaximum,
            ApprenticeshipActualStartDate = firstPrice.StartDate,
            ApprenticeshipPlannedEndDate = latestPrice.EndDate,
            AccountLegalEntityId = latestEpisode.AccountLegalEntityId,
            UKPRN = latestEpisode.Ukprn
        };
    }

    public async Task<ApprenticeshipStartDate?> GetStartDate(Guid learningKey)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .FirstOrDefaultAsync(x => x.Key == learningKey);

        var episodes = apprenticeship?.Episodes.ToList();
        var prices = episodes?.SelectMany(x => x.Prices).ToList();

        var latestPrice = prices?.MaxBy(x => x.StartDate);
        if (latestPrice == null)
        {
            return null;
        }

        var latestEpisode = episodes?.MaxBy(x => x.Prices.Max(y => y.StartDate));
        if (latestEpisode == null)
        {
            return null;
        }

        var firstPrice = prices?.MinBy(x => x.StartDate);
        if (firstPrice == null)
        {
            return null;
        }

        return apprenticeship == null
            ? null
            : new ApprenticeshipStartDate
            {
                LearningKey = apprenticeship.Key,
                ActualStartDate = firstPrice.StartDate,
                PlannedEndDate = latestPrice.EndDate,
                AccountLegalEntityId = latestEpisode.AccountLegalEntityId,
                UKPRN = latestEpisode.Ukprn,
                DateOfBirth = apprenticeship.DateOfBirth,
                CourseCode = latestEpisode.TrainingCode,
                CourseVersion = latestEpisode.TrainingCourseVersion,
                SimplifiedPaymentsMinimumStartDate = Constants.SimplifiedPaymentsMinimumStartDate
            };
    }

    
    public async Task<Guid?> GetKey(string apprenticeshipHashedId)
    {
        var apprenticeship = await DbContext.Apprenticeships.FirstOrDefaultAsync(x =>
            x.ApprenticeshipHashedId == apprenticeshipHashedId);
        return apprenticeship?.Key;
    }

    public async Task<PaymentStatus?> GetPaymentStatus(Guid learningKey)
    {
        PaymentStatus? paymentStatus = null;

        try
        {
            var apprenticeship = await DbContext.Apprenticeships
                .Include(x => x.Episodes)
                .Include(x => x.FreezeRequests)
                .FirstOrDefaultAsync(x => x.Key == learningKey);

            var episodes = apprenticeship?.Episodes.ToList();
            var latestEpisode = episodes?.MaxBy(x => x.Prices.Select(x => x.StartDate));
            if (latestEpisode == null)
            {
                return null;
            }

            paymentStatus = new PaymentStatus() { IsFrozen = latestEpisode.PaymentsFrozen };

            if (paymentStatus.IsFrozen)
            {
                var activeFreezeRequest = apprenticeship!.FreezeRequests.Single(x => x.LearningKey == learningKey && !x.Unfrozen);
                paymentStatus.Reason = activeFreezeRequest.Reason;
                paymentStatus.FrozenOn = activeFreezeRequest.FrozenDateTime;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting payment status for apprenticeship {learningKey}", learningKey);
        }

        return paymentStatus;
    }

    /// <summary>
    /// Get apprenticeships with episodes for a provider
    /// </summary>
    /// <param name="ukprn">The unique provider reference number. Only apprenticeships where the episode with this provider reference will be returned.</param>
    /// <param name="activeOnDate">If populated, will return only apprenticeships that are active on this date</param>
    public async Task<List<LearningWithEpisodes>?> GetLearningsWithEpisodes(long ukprn, DateTime? activeOnDate = null)
    {
        List<LearningWithEpisodes>? apprenticeshipWithEpisodes = null;

        try
        {
            var withdrawFromStartReason = WithdrawReason.WithdrawFromStart.ToString();
            var withdrawFromPrivateBeta = WithdrawReason.WithdrawFromBeta.ToString();

            var apprenticeships = await DbContext.Apprenticeships
                .Include(x => x.Episodes)
                .ThenInclude(x => x.Prices)
                .Where(x => x.Episodes.Any(e => e.Ukprn == ukprn))
                .Where(x => !activeOnDate.HasValue || 
                    x.Episodes.Any(episode =>
                        episode.Prices.Any(price => price.EndDate >= activeOnDate.Value.StartOfCurrentAcademicYear()) && // end date is at least after the start of this academic year
                         episode.Prices.Any(price => price.StartDate <= activeOnDate.Value)     // start date is at least before the requested period
                ))
                .ToListAsync();

            apprenticeshipWithEpisodes = apprenticeships.Select(apprenticeship =>
                new LearningWithEpisodes(
                    apprenticeship.Key,
                    apprenticeship.Uln,
                    apprenticeship.GetStartDate(),
                    apprenticeship.GetPlannedEndDate(),
                    apprenticeship.Episodes.Select(ep =>
                            new Episode(ep.Key, ep.TrainingCode, ep.LastDayOfLearning, ep.Prices.Select(p =>
                                new EpisodePrice(p.Key, p.StartDate, p.EndDate, p.TrainingPrice, p.EndPointAssessmentPrice, p.TotalPrice, p.FundingBandMaximum)).ToList()))
                        .ToList(),
                    apprenticeship.GetAgeAtStartOfApprenticeship(),
                    apprenticeship.GetLastDayOfLearning(),
                    apprenticeship.CompletionDate)
            ).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting apprenticeships with episodes for provider UKPRN {Ukprn}", ukprn);
        }

        return apprenticeshipWithEpisodes;
    }

    public async Task<CurrentPartyIds?> GetCurrentPartyIds(Guid apprenticeshipKey)
    {
        CurrentPartyIds? currentPartyIds = null;

        try
        {
            var apprenticeship = await DbContext.Apprenticeships
                .Where(a => a.Key == apprenticeshipKey)
                .Include(a => a.Episodes)
                .SingleOrDefaultAsync();

            if (apprenticeship == null)
                return null;

            var episode = apprenticeship.GetEpisode();
            currentPartyIds = new CurrentPartyIds(episode.Ukprn, episode.EmployerAccountId, apprenticeship.ApprovalsApprenticeshipId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting current party ids for apprenticeship key {key}", apprenticeshipKey);
        }

        return currentPartyIds;
    }
}