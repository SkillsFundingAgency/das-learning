using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningReinstatedEventHandler(IMessageSession messageSession, ILogger<LearningReinstatedEventHandler> logger) : IDomainEventHandler<Domain.Events.LearningReinstatedEvent>
    {
        public async Task Handle(Domain.Events.LearningReinstatedEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Publishing LearningReinstatedEvent for reinstated Learning {LearningKey}", @event.LearningKey);

            var message = new Types.LearningReinstatedEvent
            {
                LearningKey = @event.LearningKey,
                ApprenticeshipId = @event.ApprenticeshipId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
