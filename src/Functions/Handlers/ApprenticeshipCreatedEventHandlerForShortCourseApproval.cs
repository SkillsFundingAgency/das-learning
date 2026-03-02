using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Functions.Handlers
{
    public class ApprenticeshipCreatedEventHandlerForShortCourseApproval(ICommandDispatcher commandDispatcher, ILogger<ApprenticeshipCreatedEventHandlerForShortCourseApproval> logger) : IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public async Task Handle(ApprenticeshipCreatedEvent @event, IMessageHandlerContext context)
        {
            if (@event.LearningType != LearningType.ApprenticeshipUnit) { return; }

            logger.LogInformation("Handling ApprenticeshipCreatedEvent - for GSO ShortCourse Approval");

            //todo: get the learner, by uln (if it doesn't exist, log error and exit)
            //get the first (should only be one) ShortCourse for the learner
            //mark it as approved and save (this will emit an event used in earnings)

            throw new NotImplementedException();
        }
    }
}
