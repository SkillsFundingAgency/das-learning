using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler(
    IApprenticeshipLearningRepository learningRepository,
    ILogger<RemoveLearnerCommandHandler> logger)
    : ICommandHandler<RemoveLearnerCommand, List<Guid>>
{
    public async Task<List<Guid>> Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling RemoveLearnerCommandHandler for learner key {LearnerKey}", command.LearnerKey);

        var learnings = await learningRepository.GetAllByLearnerKey(command.LearnerKey);
        if (learnings.Count == 0)
        {
            throw new KeyNotFoundException($"Learning for learner key {command.LearnerKey} not found.");
        }

        var removedLearningKeys = new List<Guid>();

        foreach (var learning in learnings)
        {
            learning.RemoveLearner();
            await learningRepository.Update(learning);
            removedLearningKeys.Add(learning.Key);
        }

        return removedLearningKeys;
    }
}
