using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler : ICommandHandler<RemoveLearnerCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<RemoveLearnerCommandHandler> _logger;

    public RemoveLearnerCommandHandler(
        ILearningRepository learningRepository,
        IMessageSession messageSession,
        ILogger<RemoveLearnerCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling RemoveLearnerCommandHandler for Learning {learningKey}", command.LearnerKey);

        var learning = await _learningRepository.Get(command.LearnerKey);
        if (learning == null)
        {
            _logger.LogWarning("No learning found for learner key {LearnerKey}", command.LearnerKey);
            throw new KeyNotFoundException($"Learning with key {command.LearnerKey} not found.");
        }

        learning.RemoveLearner();

        _logger.LogInformation("Updating repository to remove from start learner with key {LearnerKey}", command.LearnerKey);
        await _learningRepository.Update(learning);

        _logger.LogInformation("Successfully updated repository after removing learner from start with key {LearnerKey}", command.LearnerKey);

        if (learning.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            await SendEvent(learning, learning.LatestEpisode.LastDayOfLearning!.Value);
        }
    }

    private async Task SendEvent(LearningDomainModel learning, DateTime lastDayOfLearning)
    {
        _logger.LogInformation("Publishing ApprenticeshipWithdrawnEvent for {learningKey}", learning.Key);
        var message = new LearningWithdrawnEvent
        {
            LearningKey = learning.Key,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            Reason = WithdrawReason.WithdrawFromStart.ToString(),
            LastDayOfLearning = lastDayOfLearning,
            EmployerAccountId = learning.LatestEpisode.EmployerAccountId
        };

        await _messageSession.Publish(message);
    }
}
