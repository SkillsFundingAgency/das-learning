using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers
{
    public class LearningDeletedEventHandler(IMessageSession messageSession, ILogger<LearningDeletedEventHandler> logger) : IDomainEventHandler<LearningDeletedEvent>
    {
        public async Task Handle(LearningDeletedEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Publishing ApprenticeshipWithdrawnEvent for deleted Learning {LearningKey}", @event.LearningKey);

            var message = new Types.ApprenticeshipWithdrawnEvent
            {
                LearningKey = @event.LearningKey,
                ApprovalsApprenticeshipId = @event.ApprovalsApprenticeshipId,
                Reason = WithdrawReason.WithdrawFromStart.ToString(),
                LastDayOfLearning = @event.LastDayOfLearning,
                EmployerAccountId = @event.EmployerAccountId
            };

            await messageSession.Publish(message, cancellationToken);
        }
    }
}
