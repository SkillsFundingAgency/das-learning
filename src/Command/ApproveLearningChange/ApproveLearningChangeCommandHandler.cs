using System.Globalization;
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

        if (!episode.FurtherApprovalNeeded)
        {
            logger.LogInformation("Episode {EpisodeKey} on learning {LearningKey} needs no further approval; approval for apprenticeship {ApprenticeshipId} ignored",
                episode.Key, command.LearningKey, command.ApprenticeshipId);
            return;
        }

        foreach (var change in command.Changes)
        {
            var current = CurrentValueOf(change.ChangeType, episode);
            if (change.New != current)
            {
                logger.LogWarning("Approved {ChangeType} {ApprovedValue} does not match current value {CurrentValue} on episode {EpisodeKey}, learning {LearningKey}; approval for apprenticeship {ApprenticeshipId} dropped",
                    change.ChangeType, change.New, current, episode.Key, command.LearningKey, command.ApprenticeshipId);
                return;
            }
        }

        learning.ClearFurtherApprovalNeeded(episode.Key);

        await learningService.UpdateLearning(learning);
    }

    private static string? CurrentValueOf(string changeType, ApprenticeshipEpisodeDomainModel episode) => changeType switch
    {
        nameof(CocChangeField.StartDate) => FormatDate(episode.FirstPrice.StartDate),
        _ => null
    };

    private static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
