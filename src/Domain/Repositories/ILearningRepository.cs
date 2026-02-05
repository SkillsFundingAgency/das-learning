using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IApprenticeshipLearningRepository
{
    Task Add(ApprenticeshipLearningDomainModel learning);
    Task<ApprenticeshipLearningDomainModel> Get(Guid key);
    Task<ApprenticeshipLearningDomainModel?> GetByUln(string uln);
    Task<ApprenticeshipLearningDomainModel?> Get(string uln, long approvalsApprenticeshipId);
    Task Update(ApprenticeshipLearningDomainModel learning);
}