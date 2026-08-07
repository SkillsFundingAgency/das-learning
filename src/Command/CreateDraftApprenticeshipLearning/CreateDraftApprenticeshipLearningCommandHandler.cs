using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommandHandler : ICommandHandler<CreateDraftApprenticeshipLearningCommand, CreateDraftApprenticeshipLearningCommandResult?>
{
    private readonly ILearnerFactory _learnerFactory;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IApprenticeshipLearningRepository _apprenticeshipLearningRepository;
    private readonly ILogger<CreateDraftApprenticeshipLearningCommandHandler> _logger;

    public CreateDraftApprenticeshipLearningCommandHandler(
        ILearnerFactory learnerFactory,
        IApprenticeshipLearningFactory learningFactory,
        ILearnerRepository learnerRepository,
        IApprenticeshipLearningRepository apprenticeshipLearningRepository,
        ILogger<CreateDraftApprenticeshipLearningCommandHandler> logger)
    {
        _learnerFactory = learnerFactory;
        _learningFactory = learningFactory;
        _learnerRepository = learnerRepository;
        _apprenticeshipLearningRepository = apprenticeshipLearningRepository;
        _logger = logger;
    }

    public async Task<CreateDraftApprenticeshipLearningCommandResult?> Handle(CreateDraftApprenticeshipLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftApprenticeshipLearningCommand");

        var learner = await GetOrCreateLearner(command);
        var learning = await _apprenticeshipLearningRepository.GetByLearnerKey(learner.Key);
        if (learning != null && !learning.LatestEpisode.IsRemoved)
        {
            _logger.LogInformation("Active apprenticeship already exists for learner with ULN {Uln}", learner.Uln);
            return null;
        }

        if (learning == null)
        {
            var createResult = await CreateDraftLearning(command, learner);
            _logger.LogInformation("Successfully created draft learning with key {LearningKey}", createResult.LearningKey);
            return createResult;
        }

        var updateModel = command.LearningUpdateContext;

        _logger.LogInformation("Reinstating learning with key {LearningKey}", learning.Key); //todo is this just the reinstate journey still at this point?

        var learningChanges = learning.Update(updateModel);
        learning.LatestEpisode.SetApprovalStatus(false);
        var learnerChanges = learner.Update(updateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        _logger.LogInformation("Updating repository for learner with key {LearningKey} with changes: {Changes}", learning.Key, changes);

        learning.AddEvent(LearnerUpdatedEvent.From(learner, learning));
        if (changes.Any(x => x == Enums.LearningUpdateChanges.PersonalDetails))
        {
            var episode = learning.Episodes.Single(x => x.Ukprn == command.Ukprn);
            learner.AddEvent(PersonalDetailsChangedEvent.From(learner, learning, episode));
        }

        await _learnerRepository.Update(learner);
        await _apprenticeshipLearningRepository.Update(learning);

        _logger.LogInformation("Successfully updated learning with key {LearningKey}", learning.Key);

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

    private async Task<LearnerDomainModel> GetOrCreateLearner(CreateDraftApprenticeshipLearningCommand command)
    {
        var updateModel = command.LearningUpdateContext;
        var learnerModel = updateModel.Learner;

        var learner = await _learnerRepository.GetByUln(learnerModel.Uln);
        if (learner != null)
        {
            return learner;
        }

        var newLearner = _learnerFactory.CreateNew(
            learnerModel.Uln,
            learnerModel.DateOfBirth,
            learnerModel.FirstName,
            learnerModel.LastName,
            learnerModel.EmailAddress);

        await _learnerRepository.Add(newLearner);

        return newLearner;
    }

    private async Task<CreateDraftApprenticeshipLearningCommandResult> CreateDraftLearning(
        CreateDraftApprenticeshipLearningCommand command,
        LearnerDomainModel learner)
    {
        var updateModel = command.LearningUpdateContext;
        var cost = updateModel.OnProgrammeDetails.Costs.Single(); //todo assume single cost at draft point

        var trainingCode = updateModel.EnglishAndMathsCourses.FirstOrDefault()?.Course ?? string.Empty;

        var learning = _learningFactory.CreateNew(learner.Key);
        learning.AddEpisode(
            updateModel.ApprovalsApprenticeshipId,
            command.Ukprn,
            employerAccountId: null,
            startDate: cost.FromDate,
            endDate: updateModel.OnProgrammeDetails.ExpectedEndDate,
            totalPrice: cost.TotalPrice,
            trainingPrice: cost.TrainingPrice,
            endpointAssessmentPrice: cost.EpaoPrice,
            fundingType: FundingType.Levy,
            fundingPlatform: FundingPlatform.DAS,
            transferSenderId: null,
            legalEntityName: string.Empty,
            accountLegalEntityId: null,
            trainingCode: trainingCode,
            trainingCourseVersion: null,
            isApproved: false);

        var learningChanges = learning.Update(updateModel);
        var learnerChanges = learner.Update(updateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        if (learnerChanges.Any())
        {
            await _learnerRepository.Update(learner);
        }

        await _apprenticeshipLearningRepository.Add(learning);

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