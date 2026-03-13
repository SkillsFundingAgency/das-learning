using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Functions.Handlers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Functions.UnitTests;

[TestFixture]
public class WhenHandlingSyncLearningCommand
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenTheMappedCommandIsSent()
    {
        var innerEvent = _fixture.Create<ApprenticeshipCreatedEvent>();
        var message = new SyncLearningCommand { InnerEvent = innerEvent };

        var dispatcher = new Mock<ICommandDispatcher>();

        var handler = new SyncLearningCommandHandler(dispatcher.Object, Mock.Of<ILogger<SyncLearningCommandHandler>>());

        await handler.Handle(message, new TestableMessageHandlerContext());

        dispatcher.Verify(x =>
            x.Send(
                It.Is<AddLearningCommand>(c =>
                    c.ApprovalsApprenticeshipId == innerEvent.ApprenticeshipId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
