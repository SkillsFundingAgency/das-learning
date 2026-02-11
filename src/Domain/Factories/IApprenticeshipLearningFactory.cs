using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IApprenticeshipLearningFactory
{
    ApprenticeshipLearningDomainModel CreateNew(long approvalsApprenticeshipId, Guid learnerKey);
    
    ApprenticeshipLearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
}
