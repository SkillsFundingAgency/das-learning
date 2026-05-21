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
                WithdrawalDate = @event.LastDayOfLearning
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
