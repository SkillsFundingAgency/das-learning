using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IShortCourseLearningRepository
{
    Task Add(ShortCourseLearningDomainModel learning);
    Task Update(ShortCourseLearningDomainModel learning);
    Task<ShortCourseLearningDomainModel> Get(Guid key);
    Task<ShortCourseLearningDomainModel?> GetByLearnerKey(Guid learnerKey);
    Task<PagedResult<Models.Dtos.Learning>> GetByDates(long ukPrn, DateRange dates, int limit, int offset, CancellationToken cancellationToken);
}