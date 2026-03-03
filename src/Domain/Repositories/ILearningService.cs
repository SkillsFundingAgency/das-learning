using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningService
{
    Task<LearningDomainModel?> GetLearning(
        string uln,
        LearningType type,
        bool isApproved,
        long approvalsApprenticeshipId);

    Task AddLearning(LearningDomainModel model, LearningType type);

    Task UpdateLearning(LearningDomainModel model, LearningType type);
}