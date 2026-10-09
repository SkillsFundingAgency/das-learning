using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesRequestQueryHandler(
    LearningDataContext dbContext,
    ILogger<GetLearningsWithEpisodesRequestQueryHandler> logger)
    : IQueryHandler<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>
{
    public async Task<GetLearningsWithEpisodesResponse?> Handle(GetLearningsWithEpisodesRequest query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling GetLearningsWithEpisodesRequest for Ukprn: {ukprn} with {keyCount} learning keys",
            query.Ukprn, query.LearningKeys.Count);

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
                logger.LogInformation("No learnings found for {ukprn} from {keyCount} learning keys", query.Ukprn, query.LearningKeys.Count);
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
                            new LearningWithEpisodes.Episode(ep.Key, apprenticeship.TrainingCode, ep.WithdrawalDate, ep.Prices.Select(p =>
                                new LearningWithEpisodes.EpisodePrice(p.Key, p.StartDate, p.EndDate, p.TrainingPrice, p.EndPointAssessmentPrice, p.TotalPrice)).ToList()))
                        .ToList(),
                    apprenticeship.GetAgeAtStartOfApprenticeship(learner.DateOfBirth),
                    apprenticeship.GetWithdrawalDate(),
                    apprenticeship.GetEpisode().CompletionDate);
            }).ToList();

            logger.LogInformation("{numberFound} apprenticeships found for {ukprn} from {keyCount} learning keys", data.Count, query.Ukprn, query.LearningKeys.Count);

            return new GetLearningsWithEpisodesResponse { Items = data };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting apprenticeships with episodes for provider UKPRN {Ukprn}", query.Ukprn);
            return null;
        }
    }
}