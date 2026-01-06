using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ArchiveLearningHistory;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.MessageHandlers;

public class LearnerUpdatedEventHandler(
    IArchiveLearningHistoryCommandHandler archiveLearningHistoryCommandHandler,
    ILogger<LearnerUpdatedEventHandler> logger)
    : IDomainEventHandler<LearnerUpdatedEvent>
{
    public async Task Handle(
        LearnerUpdatedEvent message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{functionName} processing...", nameof(LearnerUpdatedEvent));

        logger.LogInformation("LearningKey: {Key} Received {EventName}",
            message.Key,
            nameof(LearnerUpdatedEvent));

        await archiveLearningHistoryCommandHandler.Handle(new ArchiveLearningHistoryCommand(message));
    }
}