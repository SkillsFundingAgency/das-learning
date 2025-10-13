using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningQueryRepository
{
    Task<IEnumerable<DataTransferObjects.Learning>> GetAll(long ukprn, FundingPlatform? fundingPlatform);
    Task<PagedResult<DataTransferObjects.Learning>> GetByDates(long ukprn, DateRange dates, int limit, int offset, CancellationToken cancellationToken);
    Task<Guid?> GetKeyByLearningId(long learningId);
    Task<ApprenticeshipPrice?> GetPrice(Guid learningKey);
    Task<ApprenticeshipStartDate?> GetStartDate(Guid learningKey);
    Task<Guid?> GetKey(string apprenticeshipHashedId);
    Task<PaymentStatus?> GetPaymentStatus(Guid learningKey);

    /// <summary>
    /// Get learnings with episodes for a provider
    /// </summary>
    /// <param name="ukprn">The unique provider reference number. Only learnings where the episode with this provider reference will be returned.</param>
    /// <param name="activeOnDate">If populated, will return only learnings that are active on this date</param>
    Task<List<LearningWithEpisodes>?> GetLearningsWithEpisodes(long ukprn, DateTime? activeOnDate = null);
    Task<CurrentPartyIds?> GetCurrentPartyIds(Guid apprenticeshipKey);
}