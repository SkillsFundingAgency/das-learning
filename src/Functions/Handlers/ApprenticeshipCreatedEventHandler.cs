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
            if(@event.LearningType == LearningType.ApprenticeshipUnit) { return; }

            logger.LogInformation("Handling ApprenticeshipCreatedEvent - for adding Learning");

            await commandDispatcher.Send(ApprenticeshipCreatedEventMapper.ToAddLearningCommand(@event));
        }
    }
}
