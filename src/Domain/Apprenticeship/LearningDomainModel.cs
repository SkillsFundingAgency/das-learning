using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public abstract class LearningDomainModel : AggregateRoot
{
    public abstract void Approve(ApproveLearningContext context);
    /// <summary>Clears the FurtherApprovalNeeded marker on the episode. Returns false if the learning has no such episode.</summary>
    public abstract bool ClearFurtherApprovalNeeded(Guid episodeKey);
}

public abstract class LearningDomainModel<T> : LearningDomainModel where T : Learning.DataAccess.Entities.Learning.Learning
{
    protected T _entity;
    public Guid LearnerKey => _entity.LearnerKey;

    protected LearningDomainModel(T entity)
    {
        _entity = entity;
    }
}