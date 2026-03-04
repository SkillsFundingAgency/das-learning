using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningService : ILearningService
{
    private readonly ILearningRepositoryProvider _provider;

    public LearningService(ILearningRepositoryProvider provider)
    {
        _provider = provider;
    }

    public Task<LearningDomainModel?> GetUnapprovedLearning(
        string uln,
        LearningType type,
        long approvalsApprenticeshipId)
    {
        var repo = _provider.GetRepository(type);
        return repo.GetUnapprovedLearning(uln, approvalsApprenticeshipId);
    }

    public Task AddLearning(LearningDomainModel model, LearningType type)
    {
        var repo = _provider.GetRepository(type);
        return repo.AddLearning(model);
    }

    public Task UpdateLearning(LearningDomainModel model, LearningType type)
    {
        var repo = _provider.GetRepository(type);
        return repo.UpdateLearning(model);
    }
}