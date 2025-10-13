namespace SFA.DAS.Learning.AcceptanceTests;

public class TestMessageSession : IMessageSession
{
    private readonly List<object> _publishedMessages = new List<object>();

    public void ClearEventsOfType<T>()
    {
        _publishedMessages.RemoveAll(m => m is T);
    }

    public List<T> ReceivedEvents<T>()
    {
        return _publishedMessages.OfType<T>().ToList();
    }

    public Task Publish(object message, PublishOptions publishOptions, CancellationToken cancellationToken = default)
    {
        _publishedMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Send(object message, SendOptions sendOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Send<T>(Action<T> messageConstructor, SendOptions sendOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Subscribe(Type eventType, SubscribeOptions subscribeOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Unsubscribe(Type eventType, UnsubscribeOptions unsubscribeOptions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
