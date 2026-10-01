using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IApprenticeshipLearningHistoryRepository
{
    Task Add(ApprenticeshipLearningHistory item);
}
