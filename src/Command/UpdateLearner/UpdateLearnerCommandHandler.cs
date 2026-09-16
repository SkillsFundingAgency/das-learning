using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
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
        logger.LogInformation("Handling UpdateLearnerCommand for learner with key {LearnerKey}", command.LearnerKey);

        // Get all learnings for the learner key, ukprn, and training code
        // In the case of multiple with the same course code, we can't unambiguously identify which
        // row the update applies to, so we silently ignore the update until CoC/Progression deals with it
        var candidates = await learningRepository.GetAllByLearnerKey(command.LearnerKey, command.Ukprn, command.TrainingCode);

        if (candidates.Count > 1)
        {
            logger.LogWarning("More than one learning found for learner key {LearnerKey}, ukprn {Ukprn}, training code {TrainingCode}. Ignoring update.", command.LearnerKey, command.Ukprn, command.TrainingCode);
            return new UpdateLearnerResult();
        }

        var learning = candidates.SingleOrDefault();

        if (learning == null)
        {
            logger.LogWarning("No learning found for learner key {LearnerKey}", command.LearnerKey);
            throw new KeyNotFoundException($"Learning for learner key {command.LearnerKey} not found.");
        }

        var learner = await learnerRepository.Get(learning.LearnerKey);
        if (learner == null)
        {
            logger.LogWarning("No learner found for learner key {LearnerKey}", learning.LearnerKey);
            throw new KeyNotFoundException($"Learner with key {learning.LearnerKey} not found.");
        }

        var learningChanges = learning.Update(command.UpdateModel);
        var learnerChanges = learner.Update(command.UpdateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        if (changes.Length == 0)
        {
            logger.LogInformation("No changes detected for learner with key {LearnerKey}", command.LearnerKey);
            return new UpdateLearnerResult
            {
                Changes = [],
                AgeAtStartOfLearning = learning.AgeAtStartOfLearning(learner.ToModel()),
                LearningKey = learning.Key,
                LearningEpisodeKey = learning.LatestEpisode.Key,
                Prices = learning.LatestEpisode.EpisodePrices
                    .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                    .ToList()
            };
        }

        logger.LogInformation("Updating repository for learner with key {LearnerKey} with changes: {Changes}", command.LearnerKey, changes);

        learning.AddEvent(LearnerUpdatedEvent.From(learner, learning));
        if(changes.Any(x=>x == Enums.LearningUpdateChanges.PersonalDetails))
        {
            var episode = learning.LatestEpisode;
            learner.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, episode));
        }

        await learnerRepository.Update(learner);
        await learningRepository.Update(learning);

        logger.LogInformation("Successfully updated learning for learner with key {LearnerKey}", command.LearnerKey);

        return new UpdateLearnerResult
        {
            Changes = changes.ToList(),
            AgeAtStartOfLearning = learning.AgeAtStartOfLearning(learner.ToModel()),
            LearningKey = learning.Key,
            LearningEpisodeKey = learning.LatestEpisode.Key,
            Prices = learning.LatestEpisode.EpisodePrices
                .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                .ToList()
        };
    }

}
