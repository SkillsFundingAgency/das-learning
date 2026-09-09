using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ShortCourseLearningHistoryRepository(Lazy<LearningDataContext> lazyContext)
    : IShortCourseLearningHistoryRepository
{
    private LearningDataContext DbContext => lazyContext.Value;

    public async Task Add(ShortCourseLearningHistory item)
    {
        await DbContext.ShortCourseLearningHistories.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }
}
