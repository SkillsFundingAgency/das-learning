using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningWithdrawnEventHandler(IMessageSession messageSession, ILogger<LearningWithdrawnEventHandler> logger) : IDomainEventHandler<LearningWithdrawnEvent>
    {
        public async Task Handle(LearningWithdrawnEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            logger.LogInformation($"Publishing LearningWithdrawnEvent for Learning {@event.LearningKey}, WithdrawalDate {@event.LastDayOfLearning}");

            var message = new Types.LearningWithdrawnEvent
            {
                LearningKey = @event.LearningKey,
                ApprenticeshipId = @event.ApprovalsApprenticeshipId,
                WithdrawalDate = @event.LastDayOfLearning,
                WithdrawalReasonCode = -1, // Not currently used, so set to a default value
                Created = DateTime.UtcNow // This should ideally be the time the event was created, but since we don't have that information in the domain event, we'll use the current time
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
