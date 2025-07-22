using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.Services;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.AddLearning;

public class AddLearningCommandHandler : ICommandHandler<AddLearningCommand>
{
    private readonly ILearningFactory _learningFactory;
    private readonly ILearningRepository _learningRepository;
    private readonly IFundingBandMaximumService _fundingBandMaximumService;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<AddLearningCommandHandler> _logger;

    public AddLearningCommandHandler(
        ILearningFactory learningFactory,
        ILearningRepository learningRepository,
        IFundingBandMaximumService fundingBandMaximumService,
        IMessageSession messageSession,
        ILogger<AddLearningCommandHandler> logger)
    {
        _learningFactory = learningFactory;
        _learningRepository = learningRepository;
        _fundingBandMaximumService = fundingBandMaximumService;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(AddLearningCommand command, CancellationToken cancellationToken = default)
    {
        var existingLearning = await _learningRepository.Get(command.Uln, command.ApprovalsApprenticeshipId);
        if (existingLearning != null)
        {
            _logger.LogInformation($"Learning not created as a record already exists with ULN: {command.Uln} and ApprovalsApprenticeshipId: {command.ApprovalsApprenticeshipId}.");
            return;
        }

        _logger.LogInformation("Handling AddLearningCommand for Approvals Learning Id: {approvalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        if (command.ActualStartDate == null)
        {
            throw new Exception(
                $"{nameof(command.ActualStartDate)} for Learning ({command.ApprenticeshipHashedId} (Approvals Learning Id: {command.ApprovalsApprenticeshipId}) is null. " +
                $"Learnings funded by DAS should always have an actual start date. ");
        }
        var startDate = command.ActualStartDate.Value;
        var fundingBandMaximum = await _fundingBandMaximumService.GetFundingBandMaximum(int.Parse(command.TrainingCode), startDate);

        if (fundingBandMaximum == null)
            throw new Exception(
                $"No funding band maximum found for course {command.TrainingCode} for given date {startDate:u}. Approvals Learning Id: {command.ApprovalsApprenticeshipId}");

        var learning = _learningFactory.CreateNew(
            command.ApprovalsApprenticeshipId,
            command.Uln,
            command.DateOfBirth,
            command.FirstName,
            command.LastName,
            command.ApprenticeshipHashedId);

        learning.AddEpisode(
            command.UKPRN,
            command.EmployerAccountId,
            startDate,
            command.PlannedEndDate,
            command.TotalPrice,
            command.TrainingPrice,
            command.EndPointAssessmentPrice,
            command.FundingType,
            command.FundingPlatform,
            fundingBandMaximum.Value,
            command.FundingEmployerAccountId,
            command.LegalEntityName,
            command.AccountLegalEntityId,
            command.TrainingCode,
            command.TrainingCourseVersion);

        try
        {
            await _learningRepository.Add(learning);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            //2627: violation of unique constraint. 2601: violation of unique index
            _logger.LogWarning($"Unique constraint violation, uln: {command.Uln}, approvals apprenticeship id: {command.ApprovalsApprenticeshipId}.");
            return;
        }

        if (learning.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            await SendEvent(learning);
        }
    }

    private async Task SendEvent(LearningDomainModel learning)
    {
        _logger.LogInformation("Sending LearningCreatedEvent for ApprovalsApprenticeshipId: {approvalsApprenticeshipId}", learning.ApprovalsApprenticeshipId);
        var learningCreatedEvent = new LearningCreatedEvent
        {
            LearningKey = learning.Key,
            Uln = learning.Uln,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            DateOfBirth = learning.DateOfBirth,
            FirstName = learning.FirstName,
            LastName = learning.LastName,
            Episode = learning.BuildEpisodeForIntegrationEvent()
        };

        await _messageSession.Publish(learningCreatedEvent);
    }
}