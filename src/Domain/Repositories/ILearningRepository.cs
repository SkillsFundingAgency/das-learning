using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningRepository
{
    Task<LearningDomainModel?> GetLearning(string uln, long apprenticeshipId);
    Task AddLearning(LearningDomainModel model);
    Task UpdateLearning(LearningDomainModel model);
}