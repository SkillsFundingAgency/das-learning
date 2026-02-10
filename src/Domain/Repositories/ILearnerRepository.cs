using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearnerRepository
{
    Task Add(LearnerDomainModel learner);
    Task<LearnerDomainModel> GetByUln(Guid learnerkey);
    Task Update(LearnerDomainModel learning);
}
