using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Validators;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;
using SFA.DAS.Learning.Infrastructure.Services;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.WithdrawLearning;

public class WithdrawLearningCommandHandler : ICommandHandler<WithdrawLearningCommand, Outcome>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IApprenticeshipsOuterApiClient _apprenticeshipsOuterApiClient;
    private readonly ISystemClockService _systemClockService;
    private readonly IValidator<WithdrawDomainRequest> _validator;
    private readonly IMessageSession _messageSession;
    private ILogger<WithdrawLearningCommandHandler> _logger;

    public WithdrawLearningCommandHandler(
        ILearningRepository learningRepository, 
        IApprenticeshipsOuterApiClient apprenticeshipsOuterApiClient,
        ISystemClockService systemClockService,
        IValidator<WithdrawDomainRequest> validator,
        IMessageSession messageSession,
        ILogger<WithdrawLearningCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _apprenticeshipsOuterApiClient = apprenticeshipsOuterApiClient;
        _systemClockService = systemClockService;
        _validator = validator;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task<Outcome> Handle(WithdrawLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Handling WithdrawLearningCommand for ULN {command.ULN}");
        var learning = await _learningRepository.GetByUln(command.ULN);

        if (learning == null)
        {
            throw new InvalidOperationException($"Unable to find learning by ULN {command.ULN}");
        }

        var academicYear = await _apprenticeshipsOuterApiClient.GetAcademicYear(_systemClockService.UtcNow.DateTime);

        if (!_validator.IsValid(command.ToDomainRequest(), out var message, learning, academicYear.EndDate))
        {
            return Outcome.Fail(message);
        }

        _logger.LogInformation($"Validation passed, Withdrawing apprenticeship for ULN {command.ULN}");

        var reason = GetReason(command);
        learning.WithdrawApprenticeship(command.ProviderApprovedBy, command.LastDayOfLearning, reason, _systemClockService.UtcNow.DateTime);
        await _learningRepository.Update(learning);

        if (learning.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            await SendEvent(learning, reason, command.LastDayOfLearning);
        }

        _logger.LogInformation($"Sending Notification(s) for withdrawal of learning for ULN {command.ULN}");
        await _apprenticeshipsOuterApiClient.HandleWithdrawalNotifications(learning.Key, new HandleWithdrawalNotificationsRequest { LastDayOfLearning = command.LastDayOfLearning, Reason = command.Reason });

        _logger.LogInformation($"Learning withdrawn for ULN {command.ULN}");
        return Outcome.Success();
    }

    private string GetReason(WithdrawLearningCommand command)
    {
        if(command.Reason == WithdrawReason.Other.ToString())
        {
            return command.ReasonText;
        }

        return command.Reason;
    }

    private async Task SendEvent(LearningDomainModel learning, string reason, DateTime lastDayOfLearning)
    {
        _logger.LogInformation("Publishing ApprenticeshipWithdrawnEvent for {learningKey}", learning.Key);
        var message = new LearningWithdrawnEvent
        {
            LearningKey = learning.Key,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            Reason = reason,
            LastDayOfLearning = lastDayOfLearning,
            EmployerAccountId = learning.LatestEpisode.EmployerAccountId
        };

        await _messageSession.Publish(message);
    }
}
