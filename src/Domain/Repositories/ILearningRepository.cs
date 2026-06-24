using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningRepository
{
    Task<LearningDomainModel?> GetUnapprovedLearning(string uln, long apprenticeshipId, string? trainingCode = null);
    Task AddLearning(LearningDomainModel model);
    Task UpdateLearning(LearningDomainModel model);
}