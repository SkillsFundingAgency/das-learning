using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.MessageHandlers;

public class EmployerApprovalAppliedEventHandler(IMessageSession messageSession, ILogger<EmployerApprovalAppliedEventHandler> logger)
    : IDomainEventHandler<Domain.Events.EmployerApprovalAppliedEvent>
{
    public async Task Handle(Domain.Events.EmployerApprovalAppliedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Publishing ApprovedLearningUpdatedEvent and EpisodeApprovedEvent for learning {LearningKey}, episode {EpisodeKey}",
            @event.LearningKey, @event.EpisodeKey);

        await messageSession.Publish(new ApprovedLearningUpdatedEvent
        {
            LearningKey = @event.LearningKey,
            ApprenticeshipId = @event.ApprenticeshipId,
            LearningType = @event.LearningType.ToString(),
            Changes = @event.Changes
                .Select(x => new ApprenticeshipFieldChange
                {
                    ChangeType = x.ChangeType,
                    Data = new ApprenticeshipData { Old = x.Old, New = x.New }
                })
                .ToList()
        }, cancellationToken);

        await messageSession.Publish(new EpisodeApprovedEvent
        {
            LearnerKey = @event.LearnerKey,
            LearningKey = @event.LearningKey,
            EpisodeKey = @event.EpisodeKey
        }, cancellationToken);
    }
}
