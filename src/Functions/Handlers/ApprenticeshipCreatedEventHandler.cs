using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Functions.Handlers
{
    public class ApprenticeshipCreatedEventHandler(ICommandDispatcher commandDispatcher, ILogger<ApprenticeshipCreatedEventHandler> logger) : IHandleMessages<ApprenticeshipCreatedEvent>
    {
        public async Task Handle(ApprenticeshipCreatedEvent @event, IMessageHandlerContext context)
        {
            logger.LogInformation("Handling ApprenticeshipCreatedEvent");

            await commandDispatcher.Send(new AddLearningCommand
            {
                TrainingCode = @event.TrainingCode,
                ActualStartDate = @event.ActualStartDate,
                TotalPrice = @event.PriceEpisodes[0].Cost,
                TrainingPrice = @event.PriceEpisodes[0].TrainingPrice,
                EndPointAssessmentPrice = @event.PriceEpisodes[0].EndPointAssessmentPrice,
                ApprovalsApprenticeshipId = @event.ApprenticeshipId,
                EmployerAccountId = @event.AccountId,
                FundingEmployerAccountId = @event.TransferSenderId,
                FundingType = GetFundingType(@event),
                LegalEntityName = @event.LegalEntityName,
                PlannedEndDate = @event.EndDate,
                UKPRN = @event.ProviderId,
                Uln = @event.Uln,
                DateOfBirth = @event.DateOfBirth,
                FirstName = @event.FirstName,
                LastName = @event.LastName,
                ApprenticeshipHashedId = @event.ApprenticeshipHashedId,
                FundingPlatform = @event.IsOnFlexiPaymentPilot.HasValue ? (@event.IsOnFlexiPaymentPilot.Value ? FundingPlatform.DAS : FundingPlatform.SLD) : null,
                AccountLegalEntityId = @event.AccountLegalEntityId,
                TrainingCourseVersion = @event.TrainingCourseVersion,
                PlannedStartDate = @event.StartDate
            });
        }

        private FundingType GetFundingType(ApprenticeshipCreatedEvent @event)
        {
            if (@event.TransferSenderId.HasValue)
            {
                return FundingType.Transfer;
            }

            if (@event.ApprenticeshipEmployerTypeOnApproval == ApprenticeshipEmployerType.NonLevy)
            {
                return FundingType.NonLevy;
            }

            return FundingType.Levy;
        }
    }
}
