using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;

namespace SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

public class CheckApprovedApprenticeshipExistsQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<CheckApprovedApprenticeshipExistsRequest, CheckApprovedApprenticeshipExistsResponse>
{
    public async Task<CheckApprovedApprenticeshipExistsResponse> Handle(CheckApprovedApprenticeshipExistsRequest query, CancellationToken cancellationToken = default)
    {
        var matchingEpisodeStartDates = await dbContext.ApprenticeshipLearningDbSet
            .Join(
                dbContext.LearnersDbSet,
                al => al.LearnerKey,
                learner => learner.Key,
                (al, learner) => new { al, learner })
            .Where(x => x.learner.Uln == query.Uln)
            .SelectMany(x => x.al.Episodes
                .Where(e => e.Ukprn == query.Ukprn && e.TrainingCode == query.TrainingCode && e.IsApproved == query.IsApproved)
                .Select(e => e.Prices.Min(p => (DateTime?)p.StartDate)))
            .Where(startDate => startDate != null)
            .ToListAsync(cancellationToken);

        var exists = matchingEpisodeStartDates.Any(startDate =>
            startDate!.Value.Year == query.StartDate.Year && startDate.Value.Month == query.StartDate.Month);

        return new CheckApprovedApprenticeshipExistsResponse(exists);
    }
}
