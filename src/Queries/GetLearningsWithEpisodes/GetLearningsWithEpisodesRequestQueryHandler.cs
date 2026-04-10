using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesRequestQueryHandler(
    LearningDataContext dbContext,
    ILogger<GetLearningsWithEpisodesRequestQueryHandler> logger)
    : IQueryHandler<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>
{
    public async Task<GetLearningsWithEpisodesResponse?> Handle(GetLearningsWithEpisodesRequest query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling GetLearningsWithEpisodesRequest for Ukprn: {ukprn} CollectionYear: {collectionYear} CollectionPeriod: {collectionPeriod} Pagination Limit: {limit} Pagination Offset: {offset}",
            query.Ukprn, query.CollectionYear, query.CollectionPeriod, query.Limit, query.Offset);

        try
        {
            var activeOnDate = query.CollectionYear.GetLastDay(query.CollectionPeriod);

            var baseQuery = dbContext.ApprenticeshipLearningDbSet
                .Include(x => x.Episodes)
                .ThenInclude(x => x.Prices)
                .Where(x => x.Episodes.Any(e => e.Ukprn == query.Ukprn && e.FundingPlatform == FundingPlatform.DAS))
                .Where(x =>
                    x.Episodes.Any(episode =>
                        episode.Prices.Any(price => price.EndDate >= activeOnDate.StartOfCurrentAcademicYear()) &&
                        episode.Prices.Any(price => price.StartDate <= activeOnDate) &&
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value == episode.Prices.Min(p => p.StartDate))))
                .OrderBy(x => x.Episodes.Min(e => e.ApprovalsApprenticeshipId))
                .AsNoTracking();

            var totalItems = await baseQuery.CountAsync(cancellationToken);

            var apprenticeships = await baseQuery
                .Skip(query.Offset)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);

            if (!apprenticeships.Any())
            {
                logger.LogInformation("No learnings found for {ukprn} (Pagination Limit: {limit} Pagination Offset: {offset})", query.Ukprn, query.Limit, query.Offset);
                return null;
            }

            var data = apprenticeships.Select(apprenticeship =>
            {
                var learner = dbContext.LearnersDbSet.Single(l => l.Key == apprenticeship.LearnerKey);
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
            }).ToList();

            logger.LogInformation("{numberFound} apprenticeships found for {ukprn} (Pagination Limit: {limit} Pagination Offset: {offset})", data.Count, query.Ukprn, query.Limit, query.Offset);

            return new GetLearningsWithEpisodesResponse
            {
                Items = data,
                PageSize = query.Limit,
                Page = query.Page,
                TotalItems = totalItems
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting apprenticeships with episodes for provider UKPRN {Ukprn}", query.Ukprn);
            return null;
        }
    }
}
