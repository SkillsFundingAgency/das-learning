using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.RejectPendingPriceChange;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Queries;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.PriceHistoryControllerTests
{
    public class WhenRejectPending
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _commandDispatcher;
        private PriceHistoryController _sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _commandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new PriceHistoryController(Mock.Of<IQueryDispatcher>(), _commandDispatcher.Object, Mock.Of<ILogger<PriceHistoryController>>());
            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext();
            _sut.ControllerContext.HttpContext = httpContext;
        }

        [Test]
        public async Task ThenPendingPriceChangeIsRejected()
        {
            //  Arrange
            var apprenticeshipKey = _fixture.Create<Guid>();
            var request = _fixture.Create<PatchRejectPriceChangeRequest>();

            //  Act
            var result = await _sut.RejectPendingPriceChange(apprenticeshipKey, request);

            //  Assert
            result.Should().BeOfType<OkObjectResult>();

            _commandDispatcher
                .Verify(x =>
                    x.Send(
                        It.Is<RejectPendingPriceChangeRequest>(r => r.LearningKey == apprenticeshipKey && r.Reason == request.Reason), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}