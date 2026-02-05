using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.SetPaymentsFrozen;

public class SetPaymentsFrozenCommandHandler : ICommandHandler<SetPaymentsFrozenCommand>
{
    private readonly IApprenticeshipLearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<SetPaymentsFrozenCommandHandler> _logger;

    public SetPaymentsFrozenCommandHandler(
        IApprenticeshipLearningRepository learningRepository,
        IMessageSession messageSession,
        ILogger<SetPaymentsFrozenCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(SetPaymentsFrozenCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SetPaymentsFrozenCommand for Learning {learningKey}", command.LearningKey);
        
        var apprenticeship = await _learningRepository.Get(command.LearningKey);
        apprenticeship.SetPaymentsFrozen(command.NewPaymentsFrozenStatus, command.UserId, DateTime.Now, command.Reason);
        await _learningRepository.Update(apprenticeship);

        if (apprenticeship.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            if (command.NewPaymentsFrozenStatus)
            {
                _logger.LogInformation("Publishing PaymentsFrozenEvent for Learning {apprenticeshipKey}", command.LearningKey);
                var message = new PaymentsFrozenEvent { LearningKey = command.LearningKey };
                await _messageSession.Publish(message);

            }
            else
            {
                _logger.LogInformation("Publishing PaymentsUnfrozenEvent for Learning {learningKey}", command.LearningKey);
                var message = new PaymentsUnfrozenEvent { LearningKey = command.LearningKey };
                await _messageSession.Publish(message);
            }
        }
    }
}
