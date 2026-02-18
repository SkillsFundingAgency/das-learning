using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IApprenticeshipLearningFactory
{
    ApprenticeshipLearningDomainModel CreateNew(long approvalsApprenticeshipId, Guid learnerKey);

    ApprenticeshipLearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
}

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
