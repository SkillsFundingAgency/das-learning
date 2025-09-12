using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Validators;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Infrastructure.Services;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Validators;

public class WhenValidatingWithdrawRequest
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _systemClockService;

    private const string ValidUln = "1234567890";
    private const string InValidUln = "000000000";

    private const long ValidUkprn = 1000000;
    private const long InValidUkprn = 9999999;
    private const string StringLongerThan100 = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz";


    public WhenValidatingWithdrawRequest()
    {
        _fixture = new Fixture();
        _systemClockService = new Mock<ISystemClockService>();
        _systemClockService.Setup(x => x.UtcNow).Returns(new DateTime(2024, 11, 20));
    }

    [Test]
    public void WhenApprenticeshipIsNullThenReturnsExpectedValidationError()
    {
        // Arrange
        LearningDomainModel? apprenticeship = null;
        var validator = new WithdrawValidator(_systemClockService.Object);
        var command = new WithdrawDomainRequest
        {
            ULN = InValidUln,
            UKPRN = ValidUkprn,
            Reason = WithdrawReason.WithdrawFromStart.ToString()
        };

        // Act
        var result = validator.IsValid(command, out var validationMessage, apprenticeship, new DateTime(2025, 7, 22));

        // Assert
        result.Should().BeFalse();
        validationMessage.Should().Be($"No apprenticeship found for ULN {InValidUln}");
    }

    [Test]
    public void WhenApprenticeshipDoesNotBelongToUkprnThenReturnsExpectedValidationError()
    {
        // Arrange
        var validator = new WithdrawValidator(_systemClockService.Object);
        var apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, ukprn: ValidUkprn);
        var command = new WithdrawDomainRequest
        {
            ULN = ValidUln,
            UKPRN = InValidUkprn,
            Reason = WithdrawReason.WithdrawFromStart.ToString()
        };

        // Act
        var result = validator.IsValid(command, out var validationMessage, apprenticeship, new DateTime(2025, 7, 22));

        // Assert
        result.Should().BeFalse();
        validationMessage.Should().Be($"Learning not found for ULN {command.ULN} and UKPRN {command.UKPRN}");
    }

    [Test]
    public void WhenApprenticeshipAlreadyWithdrawnThenReturnsExpectedValidationError()
    {
        // Arrange
        var validator = new WithdrawValidator(_systemClockService.Object);
        var apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, ukprn: ValidUkprn);
        apprenticeship.WithdrawApprenticeship("ProviderApprovedBy", DateTime.UtcNow, "Reason", DateTime.UtcNow);

        var command = new WithdrawDomainRequest
        {
            ULN = ValidUln,
            UKPRN = ValidUkprn,
            Reason = WithdrawReason.WithdrawFromStart.ToString()
        };

        // Act
        var result = validator.IsValid(command, out var validationMessage, apprenticeship, new DateTime(2025, 7, 22));

        // Assert
        result.Should().BeFalse();
        validationMessage.Should().Contain("Learning already withdrawn for ULN");
    }

    [Test]
    public void WhenApprenticeshipIsWithdrawnThenCancelAnyPendingRequests()
    {
        // Arrange
        var validator = new WithdrawValidator(_systemClockService.Object);
        var apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, ukprn: ValidUkprn);
        ApprenticeshipDomainModelTestHelper.AddPendingStartDateChange(apprenticeship, ChangeInitiator.Employer, DateTime.UtcNow);

        // Act
        apprenticeship.WithdrawApprenticeship("ProviderApprovedBy", DateTime.UtcNow, "Reason", DateTime.UtcNow);

        // Assert
        apprenticeship.PendingStartDateChange.Should().BeNull();
    }

    [TestCase("Invalid reason, possible values are", "InvalidValue", "")]
    [TestCase("Reason text is required for 'Other' reason", "Other", "")]
    [TestCase("Reason text must be less than 100 characters", "Other", StringLongerThan100)]
    public void WhenInvalidReasonThenReturnsExpectedValidationError(string expectedMessage, string reason, string reasonText)
    {
        // Arrange
        var validator = new WithdrawValidator(_systemClockService.Object);
        var apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, ukprn: ValidUkprn);

        var command = new WithdrawDomainRequest
        {
            ULN = ValidUln,
            UKPRN = ValidUkprn,
            Reason = reason,
            ReasonText = reasonText
        };

        // Act
        var result = validator.IsValid(command, out var validationMessage, apprenticeship, new DateTime(2025, 7, 22));

        // Assert
        result.Should().BeFalse();
        validationMessage.Should().Contain(expectedMessage);
    }

    [TestCase("LastDayOfLearning cannot be before the start date", "2024-8-1")]
    [TestCase("LastDayOfLearning cannot be after the end of the current academic year", "2025-7-23")]
    public void WhenInvalidLastDayThenReturnsExpectedValidationError(string expectedMessage, string lastDayOfLearning)
    {
        // Arrange
        var validator = new WithdrawValidator(_systemClockService.Object);
        var apprenticeship = ApprenticeshipDomainModelTestHelper.CreateBasicTestModel();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship, startDate: new DateTime(2024, 9, 1), endDate: new DateTime(2026, 7, 15), ukprn: ValidUkprn);

        var command = new WithdrawDomainRequest
        {
            ULN = ValidUln,
            UKPRN = ValidUkprn,
            Reason = WithdrawReason.WithdrawFromStart.ToString(),
            LastDayOfLearning = DateTime.Parse(lastDayOfLearning)
        };

        // Act
        var result = validator.IsValid(command, out var validationMessage, apprenticeship, new DateTime(2025, 7, 22));

        // Assert
        result.Should().BeFalse();
        validationMessage.Should().Be(expectedMessage);
    }

}
