namespace SFA.DAS.Learning.Domain.Apprenticeship;

public abstract class LearningDomainModel<T> : AggregateRoot where T : Learning.DataAccess.Entities.Learning.Learning
{
    protected T _entity;
    public Guid LearnerKey => _entity.LearnerKey;

    protected LearningDomainModel(T entity)
    {
        _entity = entity;
    }
}