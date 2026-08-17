using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface IApprenticeshipLearningRepository : ILearningRepository
{
    Task Add(ApprenticeshipLearningDomainModel learning);
    Task<ApprenticeshipLearningDomainModel> Get(Guid key);
    Task<ApprenticeshipLearningDomainModel?> GetByUln(string uln);
    Task<ApprenticeshipLearningDomainModel?> Get(string uln, long approvalsApprenticeshipId);
    Task<ApprenticeshipLearningDomainModel?> GetByLearnerKey(Guid key);
    Task<List<ApprenticeshipLearningDomainModel>> GetAllByLearnerKey(Guid learnerKey, long? ukprn = null, string? courseCode = null);
    Task<List<ApprenticeshipLearningDomainModel>> GetOtherUnapprovedCourseLearnings(Guid learnerKey, long ukprn, string excludingCourseCode);
    Task Update(ApprenticeshipLearningDomainModel learning);
}