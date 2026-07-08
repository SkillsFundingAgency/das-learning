using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler(
    IApprenticeshipLearningRepository learningRepository,
    ILogger<RemoveLearnerCommandHandler> logger)
    : ICommandHandler<RemoveLearnerCommand>
{
    public async Task Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling RemoveLearnerCommandHandler for Learning for Learner {learnerKey}", command.LearnerKey);

        var learning = await learningRepository.GetByLearnerKey(command.LearnerKey);
        if (learning == null)
        {
            throw new KeyNotFoundException($"Learning for Learner with key {command.LearnerKey} not found.");
        }

        learning.RemoveLearner();

        await learningRepository.Update(learning);
    }
}
