using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommandHandler(
    IApprenticeshipLearningRepository learningRepository,
    ILogger<RemoveLearnerCommandHandler> logger)
    : ICommandHandler<RemoveLearnerCommand, List<RemovedLearning>>
{
    public async Task<List<RemovedLearning>> Handle(RemoveLearnerCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling RemoveLearnerCommandHandler for learner key {LearnerKey}", command.LearnerKey);

        var learnings = await learningRepository.GetAllByLearnerKey(command.LearnerKey, command.Ukprn);

        var learningsInScope = learnings.Where(x => x.OverlapsAcademicYear(command.AcademicYear)).ToList();

        if (learningsInScope.Count == 0)
        {
            throw new KeyNotFoundException($"Learning for learner key {command.LearnerKey} in {command.AcademicYear} AY not found.");
        }

        var removedLearnings = new List<RemovedLearning>();

        foreach (var learning in learningsInScope)
        {
            learning.RemoveLearner();
            learning.AddEvent(ApprenticeshipLearningChangedEvent.From(learning, command.AcademicYear, ApprenticeshipLearningOperation.Removed));
            await learningRepository.Update(learning);
            removedLearnings.Add(new RemovedLearning { LearningKey = learning.Key, LearnerRef = learning.LatestEpisode.LearnerRef });
        }

        return removedLearnings;
    }
}
