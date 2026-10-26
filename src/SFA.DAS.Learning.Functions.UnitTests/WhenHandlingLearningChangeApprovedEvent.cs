using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ApproveLearningChange;
using SFA.DAS.Learning.Functions.Handlers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Functions.UnitTests;

[TestFixture]
public class WhenHandlingLearningChangeApprovedEvent
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
        var message = _fixture.Create<LearningChangeApprovedEvent>();

        ApproveLearningChangeCommand? sentCommand = null;
        var dispatcher = new Mock<ICommandDispatcher>();
        dispatcher
            .Setup(x => x.Send(It.IsAny<ApproveLearningChangeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ApproveLearningChangeCommand, CancellationToken>((command, _) => sentCommand = command);

        var handler = new LearningChangeApprovedEventHandler(dispatcher.Object, Mock.Of<ILogger<LearningChangeApprovedEventHandler>>());

        await handler.Handle(message, new TestableMessageHandlerContext());

        sentCommand.Should().NotBeNull();
        sentCommand!.LearningKey.Should().Be(message.LearningKey);
        sentCommand.ApprenticeshipId.Should().Be(message.ApprenticeshipId);
        sentCommand.Changes.Should().BeEquivalentTo(message.Changes.Select(c => new ApprovedChange(c.ChangeType, c.Data.Old, c.Data.New)));
    }
}
