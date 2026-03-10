using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetLearningsRequest, GetLearningsResponse>
{
    public async Task<GetLearningsResponse> Handle(GetLearningsRequest query, CancellationToken cancellationToken = default)
    {
        var learnings = await dbContext.ApprenticeshipLearningDbSet
            .Where(al => al.Episodes.Any(e =>
                e.Ukprn == query.Ukprn &&
                (!query.FundingPlatform.HasValue || e.FundingPlatform == query.FundingPlatform)))
            .Join(
                dbContext.LearnersDbSet,
                al => al.LearnerKey,
                learner => learner.Key,
                (al, learner) => new Models.Dtos.Learning
                {
                    Uln = learner.Uln,
                    FirstName = learner.FirstName,
                    LastName = learner.LastName
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new GetLearningsResponse(learnings);
    }
}
