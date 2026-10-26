using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.ApproveLearningChange;

public class ApproveLearningChangeCommandHandler(
    ILearningService learningService,
    ILogger<ApproveLearningChangeCommandHandler> logger)
    : ICommandHandler<ApproveLearningChangeCommand>
{
    public async Task Handle(ApproveLearningChangeCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling ApproveLearningChangeCommand for learning {LearningKey}, apprenticeship {ApprenticeshipId}",
            command.LearningKey, command.ApprenticeshipId);

        if (await learningService.GetLearning(command.LearningKey, LearningType.Apprenticeship) is not ApprenticeshipLearningDomainModel learning)
        {
            logger.LogWarning("Learning {LearningKey} not found; approval for apprenticeship {ApprenticeshipId} dropped",
                command.LearningKey, command.ApprenticeshipId);
            return;
        }

        var episode = learning.Episodes.SingleOrDefault(x => x.ApprovalsApprenticeshipId == command.ApprenticeshipId);
        if (episode == null)
        {
            logger.LogWarning("No episode on learning {LearningKey} matches apprenticeship {ApprenticeshipId}; approval dropped",
                command.LearningKey, command.ApprenticeshipId);
            return;
        }

        learning.ClearFurtherApprovalNeeded(episode.Key);

        await learningService.UpdateLearning(learning);
    }
}
