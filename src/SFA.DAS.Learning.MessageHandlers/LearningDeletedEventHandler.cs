using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningDeletedEventHandler(IMessageSession messageSession, ILogger<LearningDeletedEventHandler> logger) : IDomainEventHandler<LearningDeletedEvent>
    {
        public async Task Handle(LearningDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Publishing LearningRemovedEvent for deleted Learning {LearningKey}", @event.LearningKey);

            var message = new Types.LearningRemovedEvent
            {
                LearningKey = @event.LearningKey,
                ApprenticeshipId = @event.ApprovalsApprenticeshipId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
