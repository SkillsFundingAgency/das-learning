using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Functions.Mappers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Functions.Handlers;

public class ApprenticeshipStoppedEventHandler(ICommandDispatcher commandDispatcher, ILogger<ApprenticeshipStoppedEventHandler> logger) : IHandleMessages<ApprenticeshipStoppedEvent>
{
    public async Task Handle(ApprenticeshipStoppedEvent @event, IMessageHandlerContext context)
    {
        logger.LogInformation("Handling ApprenticeshipStoppedEvent");
        var command = FreezeEventMappers.ToFreezeLearningCommand(@event);
        await commandDispatcher.Send(command, CancellationToken.None);
    }
}
