using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ApproveLearningChange;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Functions.Handlers;
using System;
using System.Collections.Generic;
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
        _fixture.Customize<LearningChangeEvent.Price>(c => c.With(p => p.EffectiveFrom, "2026-08-01"));
    }

    [Test]
    public async Task ThenTheMappedCommandIsSent()
    {
        var message = _fixture.Create<LearningChangeApprovedEvent>();

        var sentCommand = await Handle(message);

        sentCommand.Should().NotBeNull();
        sentCommand!.LearningKey.Should().Be(message.LearningKey);
        sentCommand.ApprenticeshipId.Should().Be(message.ApprenticeshipId);
        sentCommand.Changes.Should().BeEquivalentTo(message.Changes.Select(c => new ApprovedChange(c.ChangeType, c.Data.Old, c.Data.New)));
    }

    [Test]
    public async Task ThenPricesAreMappedWithEffectiveFromAsADate()
    {
        var message = _fixture.Build<LearningChangeApprovedEvent>()
            .With(x => x.Changes, new List<LearningChangeEvent.Change>())
            .With(x => x.Prices, new List<LearningChangeEvent.Price>
            {
                new() { TrainingPrice = 12000m, AssessmentPrice = 3000m, EffectiveFrom = "2026-08-01" },
                new() { TrainingPrice = 14000m, AssessmentPrice = null, EffectiveFrom = "2027-02-15" }
            })
            .Create();

        var sentCommand = await Handle(message);

        sentCommand!.Prices.Should().Equal(
            new ApprovedPrice(12000m, 3000m, new DateTime(2026, 8, 1)),
            new ApprovedPrice(14000m, null, new DateTime(2027, 2, 15)));
    }

    [Test]
    public async Task ThenNoPricesAreMappedAsNull()
    {
        var message = _fixture.Build<LearningChangeApprovedEvent>()
            .Without(x => x.Prices)
            .Create();

        var sentCommand = await Handle(message);

        sentCommand!.Prices.Should().BeNull();
    }

    [Test]
    public async Task ThenAnEffectiveFromNotInTheAgreedFormatThrows()
    {
        var message = _fixture.Build<LearningChangeApprovedEvent>()
            .With(x => x.Prices, new List<LearningChangeEvent.Price>
            {
                new() { TrainingPrice = 12000m, AssessmentPrice = 3000m, EffectiveFrom = "01/08/2026" }
            })
            .Create();

        var act = () => Handle(message);

        await act.Should().ThrowAsync<FormatException>();
    }

    private static async Task<ApproveLearningChangeCommand?> Handle(LearningChangeApprovedEvent message)
    {
        ApproveLearningChangeCommand? sentCommand = null;
        var dispatcher = new Mock<ICommandDispatcher>();
        dispatcher
            .Setup(x => x.Send(It.IsAny<ApproveLearningChangeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ApproveLearningChangeCommand, CancellationToken>((command, _) => sentCommand = command);

        var handler = new LearningChangeApprovedEventHandler(dispatcher.Object, Mock.Of<ILogger<LearningChangeApprovedEventHandler>>());

        await handler.Handle(message, new TestableMessageHandlerContext());

        return sentCommand;
    }
}
