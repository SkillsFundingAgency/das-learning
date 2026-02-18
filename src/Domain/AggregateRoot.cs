namespace SFA.DAS.Learning.Domain;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

    protected void AddEvent(IDomainEvent @event)
    {
        lock (_events)
        {
            _events.Add(@event);
        }
    }

    protected void AddOrReplaceEvent<T>(IDomainEvent @event)
    {
        lock (_events)
        {
            _events.RemoveAll(e => e is T);
            _events.Add(@event);
        }
    }

    public IEnumerable<IDomainEvent> FlushEvents()
    {
        lock (_events)
        {
            var events = _events.ToArray();
            _events.Clear();
            return events;
        }
    }
}
