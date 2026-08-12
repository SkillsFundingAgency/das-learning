using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Functions.Handlers;

namespace SFA.DAS.Learning.Functions.UnitTests
{
    public class WhenHandleApprovalCreatedTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public async Task ThenApprovalIsAdded()
        {
            var @event = _fixture.Create<ApprenticeshipCreatedEvent>();
            var commandDispatcher = new Mock<ICommandDispatcher>();
            var handler = new ApprenticeshipCreatedEventHandler(commandDispatcher.Object, new Mock<ILogger<ApprenticeshipCreatedEventHandler>>().Object);
            await handler.Handle(@event, new TestableMessageHandlerContext());

            commandDispatcher.Verify(x =>
                x.Send(It.Is<AddLearningCommand>(c =>
                        c.TrainingCode == @event.TrainingCode &&
                        c.ActualStartDate == @event.ActualStartDate &&
                        c.TotalPrice == @event.PriceEpisodes[0].Cost &&
                        c.TrainingPrice == @event.PriceEpisodes[0].TrainingPrice &&
                        c.EndPointAssessmentPrice == @event.PriceEpisodes[0].EndPointAssessmentPrice &&
                        c.ApprovalsApprenticeshipId == @event.ApprenticeshipId &&
                        c.EmployerAccountId == @event.AccountId &&
                        c.TransferSenderId == @event.TransferSenderId &&
                        c.LegalEntityName == @event.LegalEntityName &&
                        c.PlannedEndDate == @event.EndDate &&
                        c.UKPRN == @event.ProviderId &&
                        c.Uln == @event.Uln &&
                        c.DateOfBirth == @event.DateOfBirth &&
                        c.FirstName == @event.FirstName &&
                        c.LastName == @event.LastName &&
                        c.ApprenticeshipHashedId == @event.ApprenticeshipHashedId &&
                        c.FundingPlatform == (@event.IsOnFlexiPaymentPilot.HasValue ? (@event.IsOnFlexiPaymentPilot.Value ? FundingPlatform.DAS : FundingPlatform.SLD) : null) &&
                        c.AccountLegalEntityId == @event.AccountLegalEntityId &&
                        c.TrainingCourseVersion == @event.TrainingCourseVersion &&
                        c.PlannedStartDate == @event.StartDate &&
                        c.EmployerType == (@event.ApprenticeshipEmployerTypeOnApproval == ApprenticeshipEmployerType.Levy ? EmployerType.Levy : EmployerType.NonLevy)
                    ),
                    It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task WhenEmployerIsLevyThenEmployerTypeIsLevy()
        {
            var @event = _fixture.Build<ApprenticeshipCreatedEvent>().With(x => x.TransferSenderId, (long?)null).With(x => x.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy).Create();
            var commandDispatcher = new Mock<ICommandDispatcher>();
            var handler = new ApprenticeshipCreatedEventHandler(commandDispatcher.Object, new Mock<ILogger<ApprenticeshipCreatedEventHandler>>().Object);
            await handler.Handle(@event, new TestableMessageHandlerContext());

            commandDispatcher.Verify(x =>
                x.Send(It.Is<AddLearningCommand>(c => c.EmployerType == EmployerType.Levy),
                    It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task WhenEmployerIsNonLevyThenEmployerTypeIsNonLevy()
        {
            var @event = _fixture.Build<ApprenticeshipCreatedEvent>().With(x => x.TransferSenderId, (long?)null).With(x => x.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.NonLevy).Create();
            var commandDispatcher = new Mock<ICommandDispatcher>();
            var handler = new ApprenticeshipCreatedEventHandler(commandDispatcher.Object, new Mock<ILogger<ApprenticeshipCreatedEventHandler>>().Object);
            await handler.Handle(@event, new TestableMessageHandlerContext());

            commandDispatcher.Verify(x =>
                x.Send(It.Is<AddLearningCommand>(c => c.EmployerType == EmployerType.NonLevy),
                    It.IsAny<CancellationToken>()));
        }
    }
}