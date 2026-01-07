using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningHistoryRepository
{
    Task Add(LearningHistory item);
}