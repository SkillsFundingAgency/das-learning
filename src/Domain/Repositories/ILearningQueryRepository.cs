using SFA.DAS.Learning.Models;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningQueryRepository
{
    Task<IEnumerable<Learning.Models.Learning>> GetAll(long ukprn, FundingPlatform? fundingPlatform);
    Task<PagedResult<Learning.Models.Learning>> GetByDates(long ukprn, DateRange dates, int limit, int offset, CancellationToken cancellationToken);
    Task<Guid?> GetKeyByLearningId(long learningId);

    /// <summary>
    /// Get learnings with episodes for a provider
    /// </summary>
    /// <param name="ukprn">The unique provider reference number. Only learnings where the episode with this provider reference will be returned.</param>
    /// <param name="activeOnDate">If populated, will return only learnings that are active on this date</param>
    /// <param name="limit">pagination limit</param>
    /// <param name="offset">pagination offset</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    Task<PagedResult<LearningWithEpisodes>?> GetLearningsWithEpisodes(long ukprn, DateTime? activeOnDate = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

}