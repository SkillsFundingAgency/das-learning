namespace SFA.DAS.Learning.Domain;

public interface IUnitOfWork
{
    void Track(AggregateRoot aggregate);
    Task CommitAsync(CancellationToken cancellationToken = default);
}
