using SFA.DAS.Learning.DataAccess;

namespace SFA.DAS.Learning.Domain;

public class UnitOfWork(LearningDataContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    private readonly List<AggregateRoot> _aggregates = new();

    public void Track(AggregateRoot aggregate) => _aggregates.Add(aggregate);

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);

        foreach (dynamic @event in _aggregates.SelectMany(a => a.FlushEvents()))
            await dispatcher.Send(@event, cancellationToken);
    }
}
