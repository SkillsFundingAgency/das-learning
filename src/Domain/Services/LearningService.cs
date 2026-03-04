using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Services;

public class LearningService(ILearningRepositoryProvider provider) : ILearningService
{
    public Task<LearningDomainModel?> GetUnapprovedLearning(
        string uln,
        LearningType type,
        long approvalsApprenticeshipId)
    {
        var repo = provider.GetRepository(type);
        return repo.GetUnapprovedLearning(uln, approvalsApprenticeshipId);
    }

    public Task AddLearning(LearningDomainModel model)
    {
        var repo = provider.GetRepository(model);
        return repo.AddLearning(model);
    }

    public Task UpdateLearning(LearningDomainModel model)
    {
        var repo = provider.GetRepository(model);
        return repo.UpdateLearning(model);
    }
}