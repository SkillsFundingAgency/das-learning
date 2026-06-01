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
        logger.LogInformation("Handling RemoveLearnerCommandHandler for Learning {learningKey}", command.LearnerKey);

        var learning = await learningRepository.Get(command.LearnerKey);
        if (learning == null)
        {
            throw new KeyNotFoundException($"Learning with key {command.LearnerKey} not found.");
        }

        learning.RemoveLearner();

        await learningRepository.Update(learning);
    }
}
