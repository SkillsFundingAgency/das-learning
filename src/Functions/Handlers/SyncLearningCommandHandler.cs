using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Learning.Functions.Mappers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command;

namespace SFA.DAS.Learning.Functions.Handlers
{
    public class SyncLearningCommandHandler(ICommandDispatcher commandDispatcher, ILogger<SyncLearningCommandHandler> logger) : IHandleMessages<SyncLearningCommand>
    {
        public async Task Handle(SyncLearningCommand @event, IMessageHandlerContext context)
        {
            logger.LogInformation("Handling SyncLearningCommand");

            await commandDispatcher.Send(ApprenticeshipCreatedEventMapper.ToAddLearningCommand(@event.InnerEvent));
        }
    }
}
