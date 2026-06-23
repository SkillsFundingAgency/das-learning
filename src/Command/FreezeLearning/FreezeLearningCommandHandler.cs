using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.Command.FreezeLearning;

public class FreezeLearningCommandHandler : ICommandHandler<FreezeLearningCommand>
{
    private readonly ILogger<FreezeLearningCommandHandler> _logger;
    private readonly IShortCourseLearningRepository _shortCourseLearningRepository;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IMessageSession _messageSession;

    public FreezeLearningCommandHandler(
        ILogger<FreezeLearningCommandHandler> logger,
        IShortCourseLearningRepository shortCourseLearningRepository,
        ILearnerRepository learnerRepository,
        IMessageSession messageSession)
    {
        _logger = logger;
        _shortCourseLearningRepository = shortCourseLearningRepository;
        _learnerRepository = learnerRepository;
        _messageSession = messageSession;
    }

    public async Task Handle(FreezeLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling FreezeLearningCommand for ApprovalsApprenticeshipId {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        var shortCourseLearning = await _shortCourseLearningRepository.GetByApprovalsApprenticeshipId(command.ApprovalsApprenticeshipId);
        if (shortCourseLearning == null)
        {
            // Logged as a warning for now. When we introduce freeze for full apprenticeships, if no learning is found then we should raise an error
            _logger.LogWarning("No short course learning found for ApprovalsApprenticeshipId {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);
            return;
        }

        var learner = await _learnerRepository.Get(shortCourseLearning.GetEntity().LearnerKey);
        if (learner == null)
        {
            _logger.LogError("Learner not found for short course learning {LearningKey}", shortCourseLearning.Key);
            throw new InvalidOperationException($"Learner not found for key {shortCourseLearning.GetEntity().LearnerKey}");
        }

        var episode = shortCourseLearning.FreezeEpisodePayments(command.ApprovalsApprenticeshipId);

        await _shortCourseLearningRepository.Update(shortCourseLearning);

        await _messageSession.Publish(new PaymentsStatusUpdatedForEpisode
        {
            LearnerKey = learner.Key,
            LearningKey = shortCourseLearning.Key,
            EpisodeKey = episode.Key,
            PaymentsFrozen = true
        });
    }
}
