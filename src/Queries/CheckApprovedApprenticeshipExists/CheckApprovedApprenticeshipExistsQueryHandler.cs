using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;

namespace SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

public class CheckApprovedApprenticeshipExistsQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<CheckApprovedApprenticeshipExistsRequest, CheckApprovedApprenticeshipExistsResponse>
{
    public async Task<CheckApprovedApprenticeshipExistsResponse> Handle(CheckApprovedApprenticeshipExistsRequest query, CancellationToken cancellationToken = default)
    {
        // A break-in-learning restart adds a second episode for the same (ukprn, trainingCode)
        // combination, so matching uses the earliest price start date across all matching
        // episodes - the original episode's start date - rather than any single episode's own.
        var matchingLearningStartDates = await dbContext.ApprenticeshipLearningDbSet
            .Join(
                dbContext.LearnersDbSet,
                al => al.LearnerKey,
                learner => learner.Key,
                (al, learner) => new { al, learner })
            .Where(x => x.learner.Uln == query.Uln)
            .Select(x => x.al.Episodes
                .Where(e => e.Ukprn == query.Ukprn && e.TrainingCode == query.TrainingCode && e.IsApproved == query.IsApproved)
                .SelectMany(e => e.Prices)
                .Select(p => (DateTime?)p.StartDate)
                .Min())
            .Where(startDate => startDate != null)
            .ToListAsync(cancellationToken);

        var exists = matchingLearningStartDates.Any(startDate =>
            startDate!.Value.Year == query.StartDate.Year && startDate.Value.Month == query.StartDate.Month);

        return new CheckApprovedApprenticeshipExistsResponse(exists);
    }
}
