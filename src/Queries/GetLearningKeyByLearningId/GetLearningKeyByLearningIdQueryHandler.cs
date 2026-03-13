using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;

namespace SFA.DAS.Learning.Queries.GetLearningKeyByLearningId;

public class GetLearningKeyByLearningIdQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetLearningKeyByLearningIdRequest, GetLearningKeyByLearningIdResponse>
{
    public async Task<GetLearningKeyByLearningIdResponse> Handle(GetLearningKeyByLearningIdRequest query, CancellationToken cancellationToken = default)
    {
        var learning = await dbContext.ApprenticeshipLearningDbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ApprovalsApprenticeshipId == query.ApprenticeshipId, cancellationToken);

        return new GetLearningKeyByLearningIdResponse { LearningKey = learning?.Key };
    }
}
