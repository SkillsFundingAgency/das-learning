using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IShortCourseLearningRepository
{
    Task Add(ShortCourseLearningDomainModel learning);
    Task<ShortCourseLearningDomainModel> Get(Guid key);
}