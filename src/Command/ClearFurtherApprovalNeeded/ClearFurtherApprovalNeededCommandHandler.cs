using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Services;

namespace SFA.DAS.Learning.Command.ClearFurtherApprovalNeeded;

public class ClearFurtherApprovalNeededCommandHandler(
    ILearningService learningService,
    ILogger<ClearFurtherApprovalNeededCommandHandler> logger)
    : ICommandHandler<ClearFurtherApprovalNeededCommand>
{
    public async Task Handle(ClearFurtherApprovalNeededCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Clearing FurtherApprovalNeeded for {LearningType} learning {LearningKey}, episode {EpisodeKey}",
            command.LearningType, command.LearningKey, command.EpisodeKey);

        var learning = await learningService.GetLearning(command.LearningKey, command.LearningType)
            ?? throw new NotFoundException($"{command.LearningType} learning {command.LearningKey} not found.");

        if (!learning.ClearFurtherApprovalNeeded(command.EpisodeKey))
            throw new NotFoundException($"Episode {command.EpisodeKey} not found on {command.LearningType} learning {command.LearningKey}.");

        await learningService.UpdateLearning(learning);
    }
}
