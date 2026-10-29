using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesRequestQueryHandler(
    LearningDataContext dbContext,
    ILogger<GetLearningsWithEpisodesRequestQueryHandler> logger)
    : IQueryHandler<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>
{
    public async Task<GetLearningsWithEpisodesResponse?> Handle(GetLearningsWithEpisodesRequest query, CancellationToken cancellationToken = default)
    {
        var keyCount = query.LearningKeys.Count;

        logger.LogInformation(
            "Handling GetLearningsWithEpisodesRequest for Ukprn: {Ukprn} with {KeyCount} learning keys",
            query.Ukprn, keyCount);

        try
        {
            var apprenticeships = await dbContext.ApprenticeshipLearningDbSet
                .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x => x.Prices)
                .Where(x => query.LearningKeys.Contains(x.Key))
                .Where(x => x.Episodes.Any(e => e.Ukprn == query.Ukprn && !e.IsRemoved))
                .OrderBy(x => x.Key)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!apprenticeships.Any())
            {
                logger.LogInformation("No learnings found for {Ukprn} from {KeyCount} learning keys", query.Ukprn, keyCount);
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
                            new LearningWithEpisodes.Episode(ep.Key, apprenticeship.TrainingCode, ep.WithdrawalDate, ep.EmployerAccountId, ep.Prices.Select(p =>
                                new LearningWithEpisodes.EpisodePrice(p.Key, p.StartDate, p.EndDate, p.TrainingPrice, p.EndPointAssessmentPrice, p.TotalPrice)).ToList()))
                        .ToList(),
                    apprenticeship.GetAgeAtStartOfApprenticeship(learner.DateOfBirth),
                    apprenticeship.GetWithdrawalDate(),
                    apprenticeship.GetEpisode().CompletionDate);
            }).ToList();

            logger.LogInformation("{NumberFound} apprenticeships found for {Ukprn} from {KeyCount} learning keys", data.Count, query.Ukprn, keyCount);

            return new GetLearningsWithEpisodesResponse { Items = data };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting apprenticeships with episodes for provider UKPRN {Ukprn}", query.Ukprn);
            return null;
        }
    }
}