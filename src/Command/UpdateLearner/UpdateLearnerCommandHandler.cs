using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommandHandler(
    ILogger<UpdateLearnerCommandHandler> logger, 
    ILearnerRepository learnerRepository,
    IApprenticeshipLearningRepository learningRepository) : ICommandHandler<UpdateLearnerCommand, UpdateLearnerResult>
{
    public async Task<UpdateLearnerResult> Handle(UpdateLearnerCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateLearnerCommand for learner with key {LearnerKey}", command.LearningKey);
        
        var learning = await learningRepository.Get(command.LearningKey);

        if (learning == null)
        {
            logger.LogWarning("No learning found for learner key {LearnerKey}", command.LearningKey);
            throw new KeyNotFoundException($"Learning with key {command.LearningKey} not found.");
        }

        var learner = await learnerRepository.Get(learning.LearnerKey);
        if (learner == null)
        {
            logger.LogWarning("No learner found for learner key {LearnerKey}", learning.LearnerKey);
            throw new KeyNotFoundException($"Learner with key {learning.LearnerKey} not found.");
        }

        var eventBuilder = new LearnerUpdatedEventBuilder(learner, learning);
        var learningChanges = learning.UpdateLearnerDetails(command.UpdateModel, eventBuilder);
        var learnerChanges = learner.Update(command.UpdateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        if (changes.Length == 0)
        {
            logger.LogInformation("No changes detected for learner with key {LearnerKey}", command.LearningKey);
            return new UpdateLearnerResult
            {
                Changes = [],
                AgeAtStartOfLearning = learning.AgeAtStartOfLearning(learner.ToModel()),
                LearningEpisodeKey = learning.LatestEpisode.Key,
                Prices = learning.LatestEpisode.EpisodePrices
                    .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                    .ToList()
            };
        }

        logger.LogInformation("Updating repository for learner with key {LearnerKey} with changes: {Changes}", command.LearningKey, changes);
        await learningRepository.Update(learning);

        logger.LogInformation("Successfully updated learner with key {LearnerKey}", command.LearningKey);

        return new UpdateLearnerResult
        {
            Changes = changes.ToList(),
            AgeAtStartOfLearning = learning.AgeAtStartOfLearning(learner.ToModel()),
            LearningEpisodeKey = learning.LatestEpisode.Key,
            Prices = learning.LatestEpisode.EpisodePrices
                .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                .ToList()
        };
    }
}
