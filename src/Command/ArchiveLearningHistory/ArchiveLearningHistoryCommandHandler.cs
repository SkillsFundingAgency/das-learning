using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.ArchiveLearningHistory;

public class ArchiveLearningHistoryCommandHandler(
    ILearningHistoryRepository repository,
    ILogger<ArchiveLearningHistoryCommandHandler> logger)
    : ICommandHandler<ArchiveLearningHistoryCommand>
{
    public async Task Handle(
        ArchiveLearningHistoryCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{handler} - Started", nameof(ArchiveLearningHistoryCommandHandler));

        var json = JsonSerializer.Serialize(
            command.LearnerUpdatedEvent,
            new JsonSerializerOptions { WriteIndented = true });

        var history = new LearningHistory
        {
            Key = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            LearningId = command.LearnerUpdatedEvent.Key,
            State = json
        };

        await repository.Add(history);
    }
}