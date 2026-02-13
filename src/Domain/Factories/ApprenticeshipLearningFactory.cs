using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public class ApprenticeshipLearningFactory : IApprenticeshipLearningFactory
{
    public ApprenticeshipLearningDomainModel CreateNew(long approvalsApprenticeshipId, Guid learnerKey)
    {
        return ApprenticeshipLearningDomainModel.New(approvalsApprenticeshipId, learnerKey);
    }

    public ApprenticeshipLearningDomainModel GetExisting(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
    {
        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
