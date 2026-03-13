using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IShortCourseLearningRepository : ILearningRepository
{
    Task Add(ShortCourseLearningDomainModel learning);
    Task Update(ShortCourseLearningDomainModel learning);
    Task<ShortCourseLearningDomainModel> Get(Guid key);
    Task<ShortCourseLearningDomainModel?> GetByLearnerKey(Guid learnerKey);
}