using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommandHandler : ICommandHandler<CreateDraftApprenticeshipLearningCommand, CreateDraftApprenticeshipLearningCommandResult?>
{
    private readonly ILearnerRepository _learnerRepository;
    private readonly IApprenticeshipLearningRepository _apprenticeshipLearningRepository;
    private readonly ILogger<CreateDraftApprenticeshipLearningCommandHandler> _logger;

    public CreateDraftApprenticeshipLearningCommandHandler(
        ILearnerRepository learnerRepository,
        IApprenticeshipLearningRepository apprenticeshipLearningRepository,
        ILogger<CreateDraftApprenticeshipLearningCommandHandler> logger)
    {

        _learnerRepository = learnerRepository;
        _apprenticeshipLearningRepository = apprenticeshipLearningRepository;
        _logger = logger;
    }

    public async Task<CreateDraftApprenticeshipLearningCommandResult?> Handle(CreateDraftApprenticeshipLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftApprenticeshipLearningCommand");

        var learner = await _learnerRepository.GetByUln(command.LearningUpdateContext.Learner.Uln);
        if (learner == null)  return null;

        var learning = await _apprenticeshipLearningRepository.GetByLearnerKey(learner.Key);
        if (learning == null) return null;

        if(!learning.LatestEpisode.IsRemoved) return null;

        // At this point, we only perform an action on create if there is an existing learning that can be reinstated.
        // all other behaviours will be developed later as part of different user stories

        var updateModel = command.LearningUpdateContext;
        updateModel.LearningKey = learning.Key;// this does not come from the request and is only available if the learning already exists

        _logger.LogInformation("Reinstating learning with key {LearningKey}", updateModel.LearningKey);

        var learningChanges = learning.Update(updateModel);
        var learnerChanges = learner.Update(updateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        _logger.LogInformation("Updating repository for learner with key {LearningKey} with changes: {Changes}", updateModel.LearningKey, changes);

        learning.AddEvent(LearnerUpdatedEvent.From(learner, learning));
        if (changes.Any(x => x == Enums.LearningUpdateChanges.PersonalDetails))
        {
            var episode = learning.Episodes.Single(x => x.Ukprn == command.Ukprn);
            learner.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, episode));
        }

        await _learnerRepository.Update(learner);
        await _apprenticeshipLearningRepository.Update(learning);

        _logger.LogInformation("Successfully updated learning with key {LearningKey}", updateModel.LearningKey);

        return new CreateDraftApprenticeshipLearningCommandResult
        {
            Changes = changes.ToList(),
            LearningKey = learning.Key,
            LearningEpisodeKey = learning.LatestEpisode.Key,
            Prices = learning.LatestEpisode.EpisodePrices
                .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                .ToList()
        };

    }
}