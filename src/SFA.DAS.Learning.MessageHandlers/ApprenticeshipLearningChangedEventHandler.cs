using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.MessageHandlers;

public class ApprenticeshipLearningChangedEventHandler(
    IApprenticeshipLearningHistoryRepository repository,
    ILogger<ApprenticeshipLearningChangedEventHandler> logger)
    : IDomainEventHandler<ApprenticeshipLearningChangedEvent>
{
    // Serialize Changes enum as readable
    private static readonly JsonSerializerOptions ChangesSerializerOptions = new() { Converters = { new JsonStringEnumConverter() } };

    public async Task Handle(
        ApprenticeshipLearningChangedEvent message,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{functionName} processing...", nameof(ApprenticeshipLearningChangedEvent));

        logger.LogInformation("LearningKey: {LearningKey} Received {EventName} ({Operation})",
            message.LearningKey,
            nameof(ApprenticeshipLearningChangedEvent),
            message.Operation);

        var stateJson = JsonSerializer.Serialize(message.Snapshot, new JsonSerializerOptions { WriteIndented = true });

        var history = new ApprenticeshipLearningHistory
        {
            Key = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            LearningKey = message.LearningKey,
            AcademicYear = message.AcademicYear,
            Operation = message.Operation.ToString(),
            Changes = message.Changes.Count > 0
                ? JsonSerializer.Serialize(message.Changes, ChangesSerializerOptions)
                : null,
            State = stateJson
        };

        await repository.Add(history);
    }
}
