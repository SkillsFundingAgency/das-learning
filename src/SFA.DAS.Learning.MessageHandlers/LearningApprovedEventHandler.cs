using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers;

public class LearningApprovedEventHandler(IMessageSession messageSession, ILogger<LearningApprovedEventHandler> logger) : IDomainEventHandler<LearningApprovedEvent>
{
    public async Task Handle(LearningApprovedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        logger.LogInformation($"Sending LearningApprovedEvent for Learning {@event.LearningKey}");

        var message = new Types.LearningApprovedEvent
        {
            LearningKey = @event.LearningKey
        };

        await messageSession.Publish(message, cancellationToken);
    }
}