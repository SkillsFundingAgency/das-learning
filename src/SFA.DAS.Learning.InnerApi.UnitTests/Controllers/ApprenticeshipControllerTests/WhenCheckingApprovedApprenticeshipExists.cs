using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ApprenticeshipControllerTests;

public class WhenCheckingApprovedApprenticeshipExists
{
    private Mock<IQueryDispatcher> _queryDispatcher;
    private Mock<ICommandDispatcher> _commandDispatcher;
    private Mock<ILogger<LearningController>> _mockLogger;
    private LearningController _sut;

    [SetUp]
    public void Setup()
    {
        _queryDispatcher = new Mock<IQueryDispatcher>();
        _commandDispatcher = new Mock<ICommandDispatcher>();
        _mockLogger = new Mock<ILogger<LearningController>>();
        _sut = new LearningController(_queryDispatcher.Object, _commandDispatcher.Object, _mockLogger.Object, Mock.Of<IPagedLinkHeaderService>());
    }

    [Test]
    public async Task ThenReturnsOkWhenApprenticeshipExists()
    {
        const long ukprn = 1000;
        const string uln = "1111111111";
        const string trainingCode = "123";
        var startDate = new DateTime(2025, 9, 1);
        const bool isApproved = true;

        _queryDispatcher
            .Setup(x => x.Send<CheckApprovedApprenticeshipExistsRequest, CheckApprovedApprenticeshipExistsResponse>(
                It.Is<CheckApprovedApprenticeshipExistsRequest>(r =>
                    r.Ukprn == ukprn && r.Uln == uln && r.TrainingCode == trainingCode && r.StartDate == startDate && r.IsApproved == isApproved)))
            .ReturnsAsync(new CheckApprovedApprenticeshipExistsResponse(true));

        var result = await _sut.CheckApprovedApprenticeshipExists(ukprn, uln, trainingCode, startDate, isApproved);

        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task ThenReturnsNotFoundWhenApprenticeshipDoesNotExist()
    {
        const long ukprn = 1000;
        const string uln = "1111111111";
        const string trainingCode = "123";
        var startDate = new DateTime(2025, 9, 1);
        const bool isApproved = true;

        _queryDispatcher
            .Setup(x => x.Send<CheckApprovedApprenticeshipExistsRequest, CheckApprovedApprenticeshipExistsResponse>(It.IsAny<CheckApprovedApprenticeshipExistsRequest>()))
            .ReturnsAsync(new CheckApprovedApprenticeshipExistsResponse(false));

        var result = await _sut.CheckApprovedApprenticeshipExists(ukprn, uln, trainingCode, startDate, isApproved);

        result.Should().BeOfType<NotFoundResult>();
    }
}
