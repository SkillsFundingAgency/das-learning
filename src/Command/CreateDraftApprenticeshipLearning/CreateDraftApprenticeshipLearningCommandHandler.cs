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

        var removedLearningKey = await RemoveMissingCourseIfUnambiguous(command, learner);

        var learnings = await _apprenticeshipLearningRepository.GetAllByLearnerKey(learner.Key, ukprn: command.Ukprn, courseCode: command.TrainingCode);

        var existingLearning = SelectExistingLearningToUpdate(learnings);

        // no unapproved draft and no single unambiguous reinstatement candidate - create a new one
        if (existingLearning == null)
        {
            var isNewApprenticeshipLearner = !(await _apprenticeshipLearningRepository.GetAllByLearnerKey(learner.Key)).Any();

            var createResult = await CreateDraftLearning(command, learner);
            createResult.RemovedLearningKey = removedLearningKey;

            if (isNewApprenticeshipLearner) createResult.Changes.Add(LearningUpdateChanges.NewApprenticeshipLearner);

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
                .ToList(),
            RemovedLearningKey = removedLearningKey
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

    // Draft Removal-by-omission due to course change: if the learner has exactly one other unapproved (not-removed)
    // course draft for this UKPRN then mark it as removed. This only applies to the given AY - drafts in other AYs are not
    // removal candidates.
    // Nb. if there are multiple other unapproved courses overlapping this AY, we can't tell which (if any) is the one to remove, so we leave them all alone.
    // Nb. also, this will change when we can deal with >1 item in the payload, but for now we only ever POST one course at a time.
    private async Task<Guid?> RemoveMissingCourseIfUnambiguous(CreateDraftApprenticeshipLearningCommand command, LearnerDomainModel learner)
    {
        var otherUnapprovedLearnings = (await _apprenticeshipLearningRepository.GetOtherUnapprovedCourseLearnings(learner.Key, command.Ukprn, command.TrainingCode))
            .Where(l => l.OverlapsAcademicYear(command.AcademicYear))
            .ToList();

        if (otherUnapprovedLearnings.Count != 1)
        {
            if (otherUnapprovedLearnings.Count > 1)
            {
                _logger.LogInformation(
                    "Not removing any unapproved apprenticeship course for learner {LearnerKey} - {Count} other unapproved courses found, ambiguous which (if any) is missing",
                    learner.Key, otherUnapprovedLearnings.Count);
            }

            return null;
        }

        var missingLearning = otherUnapprovedLearnings.Single();

        _logger.LogInformation(
            "Marking missing apprenticeship course as removed for learner {LearnerKey}: learning {LearningKey}, TrainingCode {TrainingCode} - learner switched to TrainingCode {NewTrainingCode}",
            learner.Key, missingLearning.Key, missingLearning.TrainingCode, command.TrainingCode);

        missingLearning.RemoveLearner();

        await _apprenticeshipLearningRepository.Update(missingLearning);

        return missingLearning.Key;
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

        var learning = _learningFactory.CreateNew(learner.Key, trainingCode, trainingCourseVersion: null, updateModel.Delivery.LearningType.GetValueOrDefault(LearningType.Apprenticeship));
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
            transferSenderId: null,
            legalEntityName: string.Empty,
            accountLegalEntityId: null,
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