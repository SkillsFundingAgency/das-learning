using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.EventHandlers
{
    public class WithdrawalRevertedEventHandler(IMessageSession messageSession, ILogger<WithdrawalRevertedEventHandler> logger) : IDomainEventHandler<WithdrawalRevertedEvent>
    {
        public async Task Handle(WithdrawalRevertedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            logger.LogInformation($"Sending WithdrawalRevertedEvent for Learning {@event.LearningKey}");

            var message = new Types.WithdrawalRevertedEvent
            {
                LearningKey = @event.LearningKey,
                ApprovalsApprenticeshipId = @event.ApprovalsApprenticeshipId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}