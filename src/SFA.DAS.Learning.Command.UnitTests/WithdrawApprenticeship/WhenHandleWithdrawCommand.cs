using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.WithdrawLearning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Validators;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient.Calendar;
using SFA.DAS.Learning.Infrastructure.Services;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;
using LearnerStatus = SFA.DAS.Learning.Domain.Apprenticeship.LearnerStatus;

namespace SFA.DAS.Learning.Command.UnitTests.WithdrawApprenticeship;

public class WhenHandleWithdrawCommand
{
    private Fixture _fixture;
    private Mock<ILearningRepository> _apprenticeshipRepository;
    private Mock<IApprenticeshipsOuterApiClient> _apprenticeshipsOuterApiClient;
    private Mock<ISystemClockService> _systemClockService;
    private Mock<IValidator<WithdrawDomainRequest>> _validator;
    private Mock<IMessageSession> _messageSession;
    private Mock<ILogger<WithdrawLearningCommandHandler>> _logger;
    private LearningDomainModel? _apprenticeship;

    private const long ValidUkprn = 1000000;

    public WhenHandleWithdrawCommand()
    {
        _fixture = new Fixture();
        _apprenticeshipRepository = new Mock<ILearningRepository>();
        _apprenticeshipsOuterApiClient = MockOuterApiAcademicYearEnd(2025, 7, 22);
        _validator = new Mock<IValidator<WithdrawDomainRequest>>();
        _systemClockService = MockSystemClock(2024, 12, 17);
        _messageSession = new Mock<IMessageSession>();
        _logger = new Mock<ILogger<WithdrawLearningCommandHandler>>();
    }

    [Test]
    public async Task WhenInvalidRequestThenReturnsValidationError()
    {
        // Arrange
        ResetMockRepository();
        string message = "TestMessage";
        _validator.Setup(x => x.IsValid(It.IsAny<WithdrawDomainRequest>(), out message, It.IsAny<object?[]>()))
            .Returns(false);
        var sut = new WithdrawLearningCommandHandler(
            _apprenticeshipRepository.Object,
            _apprenticeshipsOuterApiClient.Object,
            _systemClockService.Object,
            _validator.Object,
            _messageSession.Object,
            _logger.Object);

        var command = _fixture.Create<WithdrawLearningCommand>();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.GetResult<string>().Should().Be("TestMessage");
    }

    [Test]
    public async Task WhenValidRequestWithdrawCompleted()
    {
        // Arrange
        ResetMockRepository();
        string message = "";
        _validator.Setup(x => x.IsValid(It.IsAny<WithdrawDomainRequest>(), out message, It.IsAny<object?[]>()))
            .Returns(true);

        _apprenticeshipsOuterApiClient.Setup(x => x.HandleWithdrawalNotifications(It.IsAny<Guid>(),
                It.IsAny<HandleWithdrawalNotificationsRequest>()))
            .Returns(() => Task.CompletedTask);

        var sut = new WithdrawLearningCommandHandler(
            _apprenticeshipRepository.Object,
            _apprenticeshipsOuterApiClient.Object,
            _systemClockService.Object,
            _validator.Object,
            _messageSession.Object,
            _logger.Object);

        var command = _fixture.Create<WithdrawLearningCommand>();

        // Act
        await sut.Handle(command);

        // Assert
        _apprenticeshipRepository.Verify(x => x.Update(
            It.Is<LearningDomainModel>(y =>
                y.GetEntity().WithdrawalRequests
                    .Count(z => z.Reason == command.Reason
                                && z.LastDayOfLearning == command.LastDayOfLearning) == 1
                && y.LatestEpisode.LearningStatus == LearnerStatus.Withdrawn)));

        _messageSession.Verify(x => x.Publish(It.Is<LearningWithdrawnEvent>(e =>
            e.Reason == command.Reason &&
            e.LastDayOfLearning == command.LastDayOfLearning &&
            e.EmployerAccountId == _apprenticeship!.LatestEpisode.EmployerAccountId
            ), It.IsAny<PublishOptions>(),
            It.IsAny<CancellationToken>()));

        _apprenticeshipsOuterApiClient.Verify(x => x.HandleWithdrawalNotifications(_apprenticeship!.Key,
            It.Is<HandleWithdrawalNotificationsRequest>(x => x.LastDayOfLearning == command.LastDayOfLearning && x.Reason == command.Reason)));
    }

    [Test]
    public async Task WhenFundingPlatformIsNotDASThenEventNotPublished()
    {
        // Arrange
        ResetMockRepository(FundingPlatform.SLD);
        string message = "";
        _validator.Setup(x => x.IsValid(It.IsAny<WithdrawDomainRequest>(), out message, It.IsAny<object?[]>()))
            .Returns(true);

        _apprenticeshipsOuterApiClient.Setup(x => x.HandleWithdrawalNotifications(It.IsAny<Guid>(),
                It.IsAny<HandleWithdrawalNotificationsRequest>()))
            .Returns(() => Task.CompletedTask);

        var sut = new WithdrawLearningCommandHandler(
            _apprenticeshipRepository.Object,
            _apprenticeshipsOuterApiClient.Object,
            _systemClockService.Object,
            _validator.Object,
            _messageSession.Object,
            _logger.Object);

        var command = _fixture.Create<WithdrawLearningCommand>();

        // Act
        await sut.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(It.IsAny<LearningWithdrawnEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void ResetMockRepository(FundingPlatform fundingPlatform = FundingPlatform.DAS)
    {
        _apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();

        ApprenticeshipDomainModelTestHelper.AddEpisode(_apprenticeship, ukprn: ValidUkprn, fundingPlatform: fundingPlatform);

        _apprenticeshipRepository.Setup(x => x.GetByUln(It.IsAny<string>())).ReturnsAsync(_apprenticeship);
    }

    private static Mock<ISystemClockService> MockSystemClock(int year, byte month, byte date)
    {
        var systemClockService = new Mock<ISystemClockService>();
        systemClockService.Setup(x => x.UtcNow).Returns(new DateTime(year, month, date));
        return systemClockService;
    }

    private static Mock<IApprenticeshipsOuterApiClient> MockOuterApiAcademicYearEnd(int year, byte month, byte date)
    {
        var apprenticeshipsOuterApiClient = new Mock<IApprenticeshipsOuterApiClient>();
        apprenticeshipsOuterApiClient.Setup(x => x.GetAcademicYear(It.IsAny<DateTime>())).ReturnsAsync(new GetAcademicYearsResponse
        {
            EndDate = new DateTime(year, month, date)
        });
        return apprenticeshipsOuterApiClient;
    }

}
