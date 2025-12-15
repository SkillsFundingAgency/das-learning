using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningHistoryRepository(Lazy<LearningDataContext> lazyContext)
    : ILearningHistoryRepository
{
    private LearningDataContext DbContext => lazyContext.Value;

    public async Task Add(LearningHistory item)
    {
        await DbContext.LearningHistories.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }
}