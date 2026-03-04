using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Functions.Mappers;

namespace SFA.DAS.Learning.Functions.Handlers
{
    public class ApprenticeshipCreatedEventHandler(ICommandDispatcher commandDispatcher, ILogger<ApprenticeshipCreatedEventHandler> logger) : IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public async Task Handle(ApprenticeshipCreatedEvent @event, IMessageHandlerContext context)
        {
            logger.LogInformation("Handling ApprenticeshipCreatedEvent");
            var command = ApprenticeshipCreatedEventMapper.ToAddLearningCommand(@event);
            await commandDispatcher.Send(command, CancellationToken.None);
        }
    }
}
