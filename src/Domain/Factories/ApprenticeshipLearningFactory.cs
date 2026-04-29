using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IApprenticeshipLearningFactory
{
    ApprenticeshipLearningDomainModel CreateNew(Guid learnerKey);

    ApprenticeshipLearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
}

public class ApprenticeshipLearningFactory : IApprenticeshipLearningFactory
{
    public ApprenticeshipLearningDomainModel CreateNew(Guid learnerKey)
    {
        return ApprenticeshipLearningDomainModel.New(learnerKey);
    }

    public ApprenticeshipLearningDomainModel GetExisting(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
    {
        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
