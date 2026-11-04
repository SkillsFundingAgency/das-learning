using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ApproveLearningChange;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Functions.Handlers;

public class LearningChangeApprovedEventHandler(ICommandDispatcher commandDispatcher, ILogger<LearningChangeApprovedEventHandler> logger) : IHandleMessages<LearningChangeApprovedEvent>
{
    public async Task Handle(LearningChangeApprovedEvent @event, IMessageHandlerContext context)
    {
        logger.LogInformation("Handling LearningChangeApprovedEvent for learning {LearningKey}, apprenticeship {ApprenticeshipId}",
            @event.LearningKey, @event.ApprenticeshipId);

        var command = new ApproveLearningChangeCommand
        {
            LearningKey = @event.LearningKey,
            ApprenticeshipId = @event.ApprenticeshipId,
            Changes = @event.Changes.Select(c => new ApprovedChange(c.ChangeType, c.Data.Old, c.Data.New)).ToList()
        };

        await commandDispatcher.Send(command, CancellationToken.None);
    }
}
