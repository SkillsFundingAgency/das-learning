using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Services;

public interface ILearningService
{
    Task<LearningDomainModel?> GetUnapprovedLearning(
        string uln,
        LearningType type,
        long approvalsApprenticeshipId);

    Task AddLearning(LearningDomainModel model);

    Task UpdateLearning(LearningDomainModel model);
}