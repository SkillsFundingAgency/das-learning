using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Domain.EventHandlers;

public class PersonalDetailsChangedEventHandler(IMessageSession messageSession, ILogger<PersonalDetailsChangedEventHandler> logger) : IDomainEventHandler<PersonalDetailsChangedEvent>
{
    public async Task Handle(PersonalDetailsChangedEvent @event, CancellationToken cancellationToken = default(CancellationToken))
    {
        logger.LogInformation($"Sending PersonalDetailsChangedEvent for Learning {@event.LearningKey}");

        var message = new Types.PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = @event.ApprovalsApprenticeshipId,
            LearningKey = @event.LearningKey,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            EmailAddress = @event.EmailAddress
        };

        await messageSession.Publish(message, cancellationToken);
    }
}