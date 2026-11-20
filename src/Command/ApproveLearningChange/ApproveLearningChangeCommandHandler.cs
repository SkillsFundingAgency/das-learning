using System.Globalization;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Enums;
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

        var anythingRecognised = false;

        foreach (var change in command.Changes)
        {
            if (!TryGetCurrentValue(change.ChangeType, episode, out var current))
            {
                logger.LogWarning("Unrecognised change type {ChangeType} in approval for apprenticeship {ApprenticeshipId} on episode {EpisodeKey}, learning {LearningKey}; not checked, approval still applied",
                    change.ChangeType, command.ApprenticeshipId, episode.Key, command.LearningKey);
                continue;
            }

            if (change.New != current)
            {
                logger.LogWarning("Approved {ChangeType} {ApprovedValue} does not match current value {CurrentValue} on episode {EpisodeKey}, learning {LearningKey}; approval for apprenticeship {ApprenticeshipId} dropped",
                    change.ChangeType, change.New, current, episode.Key, command.LearningKey, command.ApprenticeshipId);
                return;
            }

            anythingRecognised = true;
        }

        if (command.Prices != null)
        {
            if (!PricesMatch(command.Prices, episode))
            {
                logger.LogWarning("Approved prices do not match current prices on episode {EpisodeKey}, learning {LearningKey}; approval for apprenticeship {ApprenticeshipId} dropped",
                    episode.Key, command.LearningKey, command.ApprenticeshipId);
                return;
            }

            anythingRecognised = true;
        }

        if (!anythingRecognised)
        {
            logger.LogWarning("Approval for apprenticeship {ApprenticeshipId} on episode {EpisodeKey}, learning {LearningKey} has nothing recognised to apply; dropped",
                command.ApprenticeshipId, episode.Key, command.LearningKey);
            return;
        }

        learning.ApplyEmployerApproval(episode.Key, command.Changes, command.Prices);

        await learningService.UpdateLearning(learning);
    }

    private static bool TryGetCurrentValue(string changeType, ApprenticeshipEpisodeDomainModel episode, out string? current)
    {
        current = changeType switch
        {
            nameof(CocChangeField.StartDate) => FormatDate(episode.FirstPrice.StartDate),
            _ => null
        };

        return current != null;
    }

    private static bool PricesMatch(IReadOnlyCollection<ApprovedPrice> approved, ApprenticeshipEpisodeDomainModel episode)
    {
        var approvedInOrder = approved
            .OrderBy(x => x.EffectiveFrom)
            .Select(x => (x.EffectiveFrom.Date, x.TrainingPrice ?? 0, x.AssessmentPrice ?? 0));

        var currentInOrder = episode.EpisodePrices
            .OrderBy(x => x.StartDate)
            .Select(x => (x.StartDate.Date, x.TrainingPrice ?? 0, x.EndPointAssessmentPrice ?? 0));

        return approvedInOrder.SequenceEqual(currentInOrder);
    }

    private static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
