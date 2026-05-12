using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler : ICommandHandler<RemoveLearnerCommand>
{
    private readonly IApprenticeshipLearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<RemoveLearnerCommandHandler> _logger;

    public RemoveLearnerCommandHandler(
        IApprenticeshipLearningRepository learningRepository,
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
            throw new KeyNotFoundException($"Learning with key {command.LearnerKey} not found.");
        }

        learning.RemoveLearner();

        await _learningRepository.Update(learning);

        await SendEvent(learning);
    }

    private async Task SendEvent(ApprenticeshipLearningDomainModel learning)
    {
        _logger.LogInformation("Publishing LearningRemovedEvent for {learningKey}", learning.Key);
        var message = new LearningRemovedEvent
        {
            LearningKey = learning.Key,
            ApprenticeshipId = learning.LatestEpisode.ApprovalsApprenticeshipId
        };

        await _messageSession.Publish(message);
    }
}
