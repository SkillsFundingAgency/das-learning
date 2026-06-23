using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Functions.Mappers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Functions.Handlers;


public class ApprenticeshipPausedEventHandler(ICommandDispatcher commandDispatcher, ILogger<ApprenticeshipPausedEventHandler> logger) : IHandleMessages<ApprenticeshipPausedEvent>
{
    public async Task Handle(ApprenticeshipPausedEvent @event, IMessageHandlerContext context)
    {
        logger.LogInformation("Handling ApprenticeshipPausedEvent");
        var command = FreezeEventMappers.ToFreezeLearningCommand(@event);
        await commandDispatcher.Send(command, CancellationToken.None);
    }
}