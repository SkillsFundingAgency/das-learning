using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.Dtos;
using System.Linq.Expressions;
using ApprenticeshipLearningEntity = SFA.DAS.Learning.DataAccess.Entities.Learning.ApprenticeshipLearning;

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
                .IsActiveInYear(activeOnDate, activeOnDate.StartOfCurrentAcademicYear(), activeOnDate.EndOfCurrentAcademicYear())
                .OrderBy(x => x.ApprovalsApprenticeshipId)
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

public static class  LinqExtensions
{
    public static IQueryable<ApprenticeshipLearningEntity> IsActiveInYear(
        this IQueryable<ApprenticeshipLearningEntity> source, DateTime activeOnDate, DateTime startOfAcademicYear, DateTime endOfAcademicYear)
    {
        return source
                .Where(x =>
                    // Exclude if Completed before start of activeOnDate year
                    !(x.CompletionDate.HasValue && x.CompletionDate.Value < startOfAcademicYear) &&

                    x.Episodes.Any(episode =>
                        // Include if Started on or before end of activeOnDate year
                        episode.Prices.Any(price => price.StartDate <= endOfAcademicYear) &&

                        // Exclude if withdrawn before start of activeOnDate year
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value < startOfAcademicYear) &&

                        // Exclude if Withdrawn back to start
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value == episode.Prices.Min(p => p.StartDate))));
    }
}