using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningRemovedEventHandler(IMessageSession messageSession, ILogger<LearningRemovedEventHandler> logger) : IDomainEventHandler<Domain.Events.LearningRemovedEvent>
    {
        public async Task Handle(Domain.Events.LearningRemovedEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Publishing LearningRemovedEvent for removed Learning {LearningKey}", @event.LearningKey);

            var message = new Types.LearningRemovedEvent
            {
                LearningKey = @event.LearningKey,
                ApprenticeshipId = @event.ApprenticeshipId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
