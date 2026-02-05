using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.SetPaymentsFrozen;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.UnitTests.SetPaymentsFrozen;

public class WhenHandleSetPaymentsFrozenCommand
{
    private Mock<IApprenticeshipLearningRepository> _mockApprenticeshipRepository;
    private Mock<IMessageSession> _messageSession;
    private Mock<ILogger<SetPaymentsFrozenCommandHandler>> _logger;
    private SetPaymentsFrozenCommandHandler _handler;
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _mockApprenticeshipRepository = new Mock<IApprenticeshipLearningRepository>();
        _messageSession = new Mock<IMessageSession>();
        _logger = new Mock<ILogger<SetPaymentsFrozenCommandHandler>>();
        _handler = new SetPaymentsFrozenCommandHandler(
            _mockApprenticeshipRepository.Object, _messageSession.Object, _logger.Object);
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [TestCase(SetPayments.Freeze)]
    [TestCase(SetPayments.Unfreeze)]
    public async Task Handle_ShouldGetAndUpdateApprenticeship(SetPayments setPayments)
    {
        // Arrange
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        _mockApprenticeshipRepository.Setup(x => x.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        if(setPayments == SetPayments.Unfreeze)
            apprenticeship.SetPaymentsFrozen(true, _fixture.Create<string>(), DateTime.Now, _fixture.Create<string>());

        var command = new SetPaymentsFrozenCommand(
            _fixture.Create<Guid>(), _fixture.Create<string>(), setPayments, _fixture.Create<string>());

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(x => x.Get(command.LearningKey), Times.Once);
        _mockApprenticeshipRepository.Verify(x => x.Update(
            It.Is<ApprenticeshipLearningDomainModel>(y => y
                .LatestEpisode.PaymentsFrozen == command.NewPaymentsFrozenStatus)), Times.Once);

        switch(setPayments)
        {
            case SetPayments.Freeze:
                _messageSession.Verify(x => x.Publish(
                    It.Is<PaymentsFrozenEvent>(e => e.LearningKey == command.LearningKey),
                    It.IsAny<PublishOptions>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
                break;

            case SetPayments.Unfreeze:
                _messageSession.Verify(x => x.Publish(
                    It.Is<PaymentsUnfrozenEvent>(e => e.LearningKey == command.LearningKey),
                    It.IsAny<PublishOptions>(),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
                break;
        }
    }

    [TestCase(SetPayments.Freeze)]
    [TestCase(SetPayments.Unfreeze)]
    public async Task WhenFundingPlatformIsNotDASThenEventNotPublished(SetPayments setPayments)
    {
        // Arrange
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, fundingPlatform: FundingPlatform.SLD);
        _mockApprenticeshipRepository.Setup(x => x.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        if (setPayments == SetPayments.Unfreeze)
            apprenticeship.SetPaymentsFrozen(true, _fixture.Create<string>(), DateTime.Now, _fixture.Create<string>());

        var command = new SetPaymentsFrozenCommand(
            _fixture.Create<Guid>(), _fixture.Create<string>(), setPayments, _fixture.Create<string>());

        // Act
        await _handler.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(It.IsAny<PaymentsFrozenEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        _messageSession.Verify(x => x.Publish(It.IsAny<PaymentsUnfrozenEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
