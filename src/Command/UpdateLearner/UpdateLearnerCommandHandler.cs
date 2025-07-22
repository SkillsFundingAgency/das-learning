using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommandHandler : ICommandHandler<UpdateLearnerCommand, LearningUpdateChanges[]>
{
    private readonly ILogger<UpdateLearnerCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;

    public UpdateLearnerCommandHandler(ILogger<UpdateLearnerCommandHandler> logger, ILearningRepository learningRepository)
    {
        _logger = logger;
        _learningRepository = learningRepository;
    }

    public async Task<LearningUpdateChanges[]> Handle(UpdateLearnerCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateLearnerCommand for learner with key {LearnerKey}", command.LearnerKey);
        var learning = await _learningRepository.Get(command.LearnerKey);
        if (learning == null)
        {
            _logger.LogWarning("No learning found for learner key {LearnerKey}", command.LearnerKey);
            throw new KeyNotFoundException($"Learning with key {command.LearnerKey} not found.");
        }

        var changes = learning.UpdateLearnerDetails(command.UpdateModel);

        if (changes.Length == 0)
        {
            _logger.LogInformation("No changes detected for learner with key {LearnerKey}", command.LearnerKey);
            return Array.Empty<LearningUpdateChanges>();
        }

        _logger.LogInformation("Updating repository for learner with key {LearnerKey} with changes: {Changes}", command.LearnerKey, changes);
        _learningRepository.Update(learning);

        _logger.LogInformation("Successfully updated learner with key {LearnerKey}", command.LearnerKey);
        return changes;
    }
}
