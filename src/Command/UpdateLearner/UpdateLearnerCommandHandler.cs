using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommandHandler(
    ILogger<UpdateLearnerCommandHandler> logger, 
    ILearningRepository learningRepository) : ICommandHandler<UpdateLearnerCommand, UpdateLearnerResult>
{
    public async Task<UpdateLearnerResult> Handle(UpdateLearnerCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateLearnerCommand for learner with key {LearnerKey}", command.LearnerKey);
        var learning = await learningRepository.Get(command.LearnerKey);
        if (learning == null)
        {
            logger.LogWarning("No learning found for learner key {LearnerKey}", command.LearnerKey);
            throw new KeyNotFoundException($"Learning with key {command.LearnerKey} not found.");
        }

        var changes = learning.UpdateLearnerDetails(command.UpdateModel);

        if (changes.Length == 0)
        {
            logger.LogInformation("No changes detected for learner with key {LearnerKey}", command.LearnerKey);
            return new UpdateLearnerResult
            {
                Changes = [],
                AgeAtStartOfLearning = learning.AgeAtStartOfLearning,
                LearningEpisodeKey = learning.LatestEpisode.Key,
                Prices = learning.LatestEpisode.EpisodePrices
                    .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                    .ToList()
            };
        }

        logger.LogInformation("Updating repository for learner with key {LearnerKey} with changes: {Changes}", command.LearnerKey, changes);
        await learningRepository.Update(learning);

        logger.LogInformation("Successfully updated learner with key {LearnerKey}", command.LearnerKey);

        return new UpdateLearnerResult
        {
            Changes = changes.ToList(),
            AgeAtStartOfLearning = learning.AgeAtStartOfLearning,
            LearningEpisodeKey = learning.LatestEpisode.Key,
            Prices = learning.LatestEpisode.EpisodePrices
                .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                .ToList()
        };
    }
}
