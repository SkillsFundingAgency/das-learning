using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ApprenticeshipLearningHistoryRepository(Lazy<LearningDataContext> lazyContext)
    : IApprenticeshipLearningHistoryRepository
{
    private LearningDataContext DbContext => lazyContext.Value;

    public async Task Add(ApprenticeshipLearningHistory item)
    {
        await DbContext.ApprenticeshipLearningHistories.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }
}
