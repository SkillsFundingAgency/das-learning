using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Repositories;

public interface ILearnerRepository
{
    Task Add(LearnerDomainModel learner);

    /// <summary>
    /// Gets the learner by the ULN
    /// </summary>
    Task<LearnerDomainModel> GetByUln(string uln);

    /// <summary>
    /// Finds the learner by finding a learning record, then returning the associated learner
    /// </summary>
    Task<LearnerDomainModel> GetByLearningKey(Guid learningKey);

    Task Update(LearnerDomainModel learning);
}
