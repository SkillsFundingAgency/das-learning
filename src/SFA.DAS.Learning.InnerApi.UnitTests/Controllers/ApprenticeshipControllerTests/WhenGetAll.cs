using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetLearnings;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ApprenticeshipControllerTests;

public class WhenGetAll
{
    private Fixture _fixture;
    private Mock<IQueryDispatcher> _queryDispatcher;
    private Mock<ICommandDispatcher> _commandDispatcher;
    private Mock<ILogger<LearningController>> _mockLogger;
    private LearningController _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _queryDispatcher = new Mock<IQueryDispatcher>();
        _commandDispatcher = new Mock<ICommandDispatcher>();
        _mockLogger = new Mock<ILogger<LearningController>>();
        _sut = new LearningController(_queryDispatcher.Object, _commandDispatcher.Object, _mockLogger.Object, Mock.Of<IPagedLinkHeaderService>());
    }

    [TestCase(null)]
    [TestCase(FundingPlatform.SLD)]
    [TestCase(FundingPlatform.DAS)]
    public async Task ThenLearningsAreReturned(FundingPlatform? fundingPlatform)
    {
        var ukprn = _fixture.Create<long>();
        var expectedResult = _fixture.Create<GetLearningsResponse>();

        _queryDispatcher
            .Setup(x => x.Send<GetLearningsRequest, GetLearningsResponse>(It.Is<GetLearningsRequest>(r => r.Ukprn == ukprn && r.FundingPlatform == fundingPlatform)))
            .ReturnsAsync(expectedResult);

        var result = await _sut.GetAll(ukprn, fundingPlatform);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResult.Learnings);
    }
}