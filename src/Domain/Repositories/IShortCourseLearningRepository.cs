using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IShortCourseLearningRepository
{
    Task Add(ShortCourseLearningDomainModel learning);
    Task<ShortCourseLearningDomainModel> Get(Guid key);
    Task<PagedResult<Models.Dtos.Learning>> GetByDates(long ukPrn, DateRange dates, int limit, int offset, CancellationToken cancellationToken);
}