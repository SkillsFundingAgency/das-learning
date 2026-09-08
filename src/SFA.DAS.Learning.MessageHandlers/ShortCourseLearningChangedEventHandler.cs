using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.MessageHandlers;

public class ShortCourseLearningChangedEventHandler(
    IShortCourseLearningHistoryRepository repository,
    ILogger<ShortCourseLearningChangedEventHandler> logger)
    : IDomainEventHandler<ShortCourseLearningChangedEvent>
{
    public async Task Handle(
        ShortCourseLearningChangedEvent message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{functionName} processing...", nameof(ShortCourseLearningChangedEvent));

        logger.LogInformation("LearningKey: {LearningKey} Received {EventName} ({Operation})",
            message.LearningKey,
            nameof(ShortCourseLearningChangedEvent),
            message.Operation);

        // Only the Snapshot is serialized into State: it's the aggregate's own data, so a JSON diff
        // across two rows for the same LearningKey shows the short course's actual evolution — not
        // request-context noise like AcademicYear/Operation, which live in their own columns instead.
        var stateJson = JsonSerializer.Serialize(message.Snapshot, new JsonSerializerOptions { WriteIndented = true });

        var history = new ShortCourseLearningHistory
        {
            Key = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            LearningKey = message.LearningKey,
            AcademicYear = message.AcademicYear,
            Operation = message.Operation.ToString(),
            Changes = message.Changes.Count > 0
                ? JsonSerializer.Serialize(message.Changes)
                : null,
            State = stateJson
        };

        await repository.Add(history);
    }
}
