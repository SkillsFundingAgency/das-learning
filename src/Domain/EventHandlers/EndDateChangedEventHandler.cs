using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.EventHandlers
{
    public class EndDateChangedEventHandler(IMessageSession messageSession, ILogger<EndDateChangedEventHandler> logger) : IDomainEventHandler<EndDateChangedEvent>
    {
        public async Task Handle(EndDateChangedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            logger.LogInformation($"Publishing EndDateChangedEvent for Learning {@event.LearningKey}, ApprovalsApprenticeshipId {@event.ApprovalsApprenticeshipId}, PlannedEndDate: {@event.PlannedEndDate}");

            var message = new Types.EndDateChangedEvent
            {
                ApprovalsApprenticeshipId = @event.ApprovalsApprenticeshipId,
                LearningKey = @event.LearningKey,
                PlannedEndDate = @event.PlannedEndDate
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
