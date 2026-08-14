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
        var learnings = await _apprenticeshipLearningRepository.GetAllByLearnerKey(learner.Key, ukprn: command.Ukprn, courseCode: command.TrainingCode);

        var existingLearning = SelectExistingLearningToUpdate(learnings);

        // no unapproved draft and no single unambiguous reinstatement candidate - create a new one
        if (existingLearning == null)
        {
            var createResult = await CreateDraftLearning(command, learner);
            _logger.LogInformation("Successfully created draft learning with key {LearningKey}", createResult.LearningKey);
            return createResult;
        }

        // otherwise update the existing unapproved (or reinstated) apprenticeship with new details
        var updateModel = command.LearningUpdateContext;

        var learningChanges = existingLearning.Update(updateModel);
        var learnerChanges = learner.Update(updateModel);
        var changes = learningChanges.Concat(learnerChanges).ToArray();

        _logger.LogInformation("Updating repository for learner with key {LearningKey} with changes: {Changes}", existingLearning.Key, changes);

        existingLearning.AddEvent(LearnerUpdatedEvent.From(learner, existingLearning));
        if (changes.Any(x => x == LearningUpdateChanges.PersonalDetails))
        {
            var episode = existingLearning.Episodes.Single(x => x.Ukprn == command.Ukprn);
            learner.AddEvent(PersonalDetailsChangedEvent.From(learner, existingLearning, episode));
        }

        await _learnerRepository.Update(learner);
        await _apprenticeshipLearningRepository.Update(existingLearning);

        _logger.LogInformation("Successfully updated learning with key {LearningKey}", existingLearning.Key);

        return new CreateDraftApprenticeshipLearningCommandResult
        {
            Changes = changes.ToList(),
            LearningKey = existingLearning.Key,
            LearningEpisodeKey = existingLearning.LatestEpisode.Key,
            Prices = existingLearning.LatestEpisode.EpisodePrices
                .Select(x => (UpdateLearnerResult.EpisodePrice)x)
                .ToList()
        };
    }

    private static ApprenticeshipLearningDomainModel? SelectExistingLearningToUpdate(List<ApprenticeshipLearningDomainModel> learnings)
    {
        // an unapproved draft can't legitimately pile up - if it ever does, that's an invariant break worth surfacing, not silently resolving
        var unapprovedCandidate = learnings.SingleOrDefault(l => !l.LatestEpisode.IsApproved);
        if (unapprovedCandidate != null)
        {
            return unapprovedCandidate;
        }

        // reinstatement candidates are always approved-and-removed - drafts are never returned by GET, so SLD can never DELETE one
        var reinstatementCandidates = learnings.Where(l => l.LatestEpisode.IsApproved && l.LatestEpisode.IsRemoved).ToList();

        // exactly one unambiguous candidate: reinstate it. Zero, or more than one (can't tell which is the "right" one to resume): create a fresh row instead of guessing
        return reinstatementCandidates.Count == 1 ? reinstatementCandidates.Single() : null;
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
        var cost = updateModel.OnProgrammeDetails.Costs.Single(); //assume single cost at draft point

        var trainingCode = command.TrainingCode;

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
            employerType: EmployerType.Levy,
            fundingPlatform: FundingPlatform.SLD,
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