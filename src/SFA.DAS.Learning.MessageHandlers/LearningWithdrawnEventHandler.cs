using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningWithdrawnEventHandler(IMessageSession messageSession, ILogger<LearningWithdrawnEventHandler> logger) : IDomainEventHandler<LearningWithdrawnEvent>
    {
        public async Task Handle(LearningWithdrawnEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            logger.LogInformation($"Publishing ApprenticeshipWithdrawnEvent for Learning {@event.LearningKey}, LastDayOfLearning {@event.LastDayOfLearning}");

            var message = new Types.ApprenticeshipWithdrawnEvent
            {
                LearningKey = @event.LearningKey,
                ApprovalsApprenticeshipId = @event.ApprovalsApprenticeshipId,
                Reason = @event.Reason,
                LastDayOfLearning = @event.LastDayOfLearning,
                EmployerAccountId = @event.EmployerAccountId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
