using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearningRepositoryProvider
{
    ILearningRepository GetRepository(LearningType type);
}