using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.AddLearning;

public class AddLearningCommandHandler : ICommandHandler<AddLearningCommand>
{
    private readonly ILearningService _learningService;
    private readonly ILearnerFactory _learnerFactory;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<AddLearningCommandHandler> _logger;

    public AddLearningCommandHandler(
        ILearningService learningService,
        ILearnerFactory learnerFactory,
        IApprenticeshipLearningFactory learningFactory,
        ILearnerRepository learnerRepository,
        IMessageSession messageSession,
        ILogger<AddLearningCommandHandler> logger)
    {
        _learningService = learningService;
        _learnerFactory = learnerFactory;
        _learningFactory = learningFactory;
        _learnerRepository = learnerRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(AddLearningCommand command, CancellationToken cancellationToken = default)
    {
        var existingLearning = await _learningService.GetUnapprovedLearning(command.Uln, command.LearningType, command.ApprovalsApprenticeshipId);

        if (existingLearning != null && command.LearningType == LearningType.ApprenticeshipUnit)
        {
            _logger.LogInformation("Approving unapproved ShortCourse for ULN {Uln}", command.Uln);

            ((ShortCourseLearningDomainModel)existingLearning).Approve(command.EmployerAccountId, command.EmployerType, command.ApprovalsApprenticeshipId, command.TransferSenderId);
            await _learningService.UpdateLearning(existingLearning);
            return;
        }

        if (command.LearningType == LearningType.ApprenticeshipUnit)
        {
            _logger.LogWarning("Unable to approve ShortCourse for ULN {Uln} - no ShortCourse was found", command.Uln);
            return;
        }

        if (existingLearning != null)
        {
            _logger.LogWarning("Learning not created as a record already exists with given ULN and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.", command.ApprovalsApprenticeshipId);
            return;
        }

        var learner = await GetOrCreateLearner(command);

        _logger.LogInformation("Handling AddLearningCommand for Approvals Learning Id: {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        var learning = _learningFactory.CreateNew(learner.Key);

        learning.AddEpisode(
            command.ApprovalsApprenticeshipId,
            command.UKPRN,
            command.EmployerAccountId,
            command.ActualStartDate ?? command.PlannedStartDate,
            command.PlannedEndDate,
            command.TotalPrice,
            command.TrainingPrice,
            command.EndPointAssessmentPrice,
            command.FundingType,
            command.FundingPlatform,
            command.TransferSenderId,
            command.LegalEntityName,
            command.AccountLegalEntityId,
            command.TrainingCode,
            command.TrainingCourseVersion);

        learning.AddUpdatedEvent(LearnerUpdatedEvent.From(learner, learning));

        try
        {
            await _learningService.AddLearning(learning);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            //2627: violation of unique constraint. 2601: violation of unique index
            _logger.LogWarning(ex,
                "Unique constraint violation for given Uln and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.",
                command.ApprovalsApprenticeshipId);
            return;
        }

        if (learning.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            await SendEvent(learning, learner);
        }
    }

    private async Task<LearnerDomainModel> GetOrCreateLearner(AddLearningCommand command)
    {
        var learner = await _learnerRepository.GetByUln(command.Uln);

        if (learner != null)
        {
            return learner;
        }

        try
        {
            var newLearner =
                _learnerFactory.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName);
            await _learnerRepository.Add(newLearner);
            return newLearner;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            //2627: violation of unique constraint. 2601: violation of unique index
            _logger.LogWarning(ex,
                "Unique constraint violation for given Uln {Uln}.", command.Uln);
            throw; //rethrowing will allow the command to be retried. Learner duplication will then be avoided, allowing the new learning to be recorded
        }
    }

    private async Task SendEvent(ApprenticeshipLearningDomainModel learning, LearnerDomainModel learner)
    {

        _logger.LogInformation(
            "Sending LearningCreatedEvent for ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.",
            learning.LatestEpisode.ApprovalsApprenticeshipId);
        var learningCreatedEvent = new LearningCreatedEvent
        {
            LearningKey = learning.Key,
            Uln = learner.Uln,
            ApprovalsApprenticeshipId = learning.LatestEpisode.ApprovalsApprenticeshipId,
            DateOfBirth = learner.DateOfBirth,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            Episode = learning.BuildEpisodeForIntegrationEvent(learner)
        };

        await _messageSession.Publish(learningCreatedEvent);
    }
}