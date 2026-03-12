using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.AddLearning;

public class AddLearningCommandHandler : ICommandHandler<AddLearningCommand>
{
    private readonly ILearnerFactory _learnerFactory;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IApprenticeshipLearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<AddLearningCommandHandler> _logger;

    public AddLearningCommandHandler(
        ILearnerFactory learnerFactory,
        IApprenticeshipLearningFactory learningFactory,
        ILearnerRepository learnerRepository,
        IApprenticeshipLearningRepository learningRepository,
        IMessageSession messageSession,
        ILogger<AddLearningCommandHandler> logger)
    {
        _learnerFactory = learnerFactory;
        _learningFactory = learningFactory;
        _learnerRepository = learnerRepository;
        _learningRepository = learningRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(AddLearningCommand command, CancellationToken cancellationToken = default)
    {
        var existingLearning = await _learningRepository.Get(command.Uln, command.ApprovalsApprenticeshipId);
        
        if (existingLearning != null)
        {
            _logger.LogInformation("Learning not created as a record already exists with given ULN and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.", command.ApprovalsApprenticeshipId);
            return;
        }

        var learner = await GetOrCreateLearner(command);

        _logger.LogInformation("Handling AddLearningCommand for Approvals Learning Id: {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        var learning = _learningFactory.CreateNew(command.ApprovalsApprenticeshipId, learner.Key);

        learning.AddEpisode(
            command.UKPRN,
            command.EmployerAccountId,
            command.ActualStartDate ?? command.PlannedStartDate,
            command.PlannedEndDate,
            command.TotalPrice,
            command.TrainingPrice,
            command.EndPointAssessmentPrice,
            command.FundingType,
            command.FundingPlatform,
            command.FundingEmployerAccountId,
            command.LegalEntityName,
            command.AccountLegalEntityId,
            command.TrainingCode,
            command.TrainingCourseVersion);

        learning.AddUpdatedEvent(LearnerUpdatedEvent.From(learner, learning));

        try
        {
            await _learningRepository.Add(learning);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            //2627: violation of unique constraint. 2601: violation of unique index
            _logger.LogWarning(
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
            _logger.LogWarning(
                "Unique constraint violation for given Uln {Uln}.", command.Uln);
            throw; //rethrowing will allow the command to be retried. Learner duplication will then be avoided, allowing the new learning to be recorded
        }
    }

    private async Task SendEvent(ApprenticeshipLearningDomainModel learning, LearnerDomainModel learner)
    {

        _logger.LogInformation(
            "Sending LearningCreatedEvent for ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.",
            learning.ApprovalsApprenticeshipId);
        var learningCreatedEvent = new LearningCreatedEvent
        {
            LearningKey = learning.Key,
            Uln = learner.Uln,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            DateOfBirth = learner.DateOfBirth,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            Episode = learning.BuildEpisodeForIntegrationEvent(learner)
        };

        await _messageSession.Publish(learningCreatedEvent);
    }
}