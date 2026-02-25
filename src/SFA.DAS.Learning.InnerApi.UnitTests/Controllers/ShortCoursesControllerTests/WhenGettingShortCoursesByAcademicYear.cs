using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ShortCoursesControllerTests
{
    public class WhenGettingShortCoursesByAcademicYear
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ILogger<ShortCoursesController>> _mockLogger;
        private Mock<IPagedLinkHeaderService> _mockPagedLinkHeaderService;
        private ShortCoursesController _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockLogger = new Mock<ILogger<ShortCoursesController>>();
            _mockPagedLinkHeaderService = new Mock<IPagedLinkHeaderService>();

            _sut = new ShortCoursesController(
                _mockQueryDispatcher.Object,
                _mockCommandDispatcher.Object,
                _mockLogger.Object,
                _mockPagedLinkHeaderService.Object
            );

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Test]
        public async Task ThenReturnsOkWithResponse()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            var academicYear = 2425;
            var expectedResponse = _fixture.Create<GetShortCoursesByAcademicYearResponse>();

            _mockQueryDispatcher
                .Setup(x => x.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>()))
                .ReturnsAsync(expectedResponse);

            _mockPagedLinkHeaderService
                .Setup(x => x.GetPageLinks(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>(),
                    It.IsAny<GetShortCoursesByAcademicYearResponse>()))
                .Returns(new KeyValuePair<string, StringValues>("Link", StringValues.Empty));

            // Act
            var result = await _sut.GetByAcademicYear(ukprn, academicYear);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Test]
        public async Task ThenDispatchesRequestWithCorrectParameters()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            var academicYear = 2425;
            var page = 2;
            var pageSize = 50;

            _mockQueryDispatcher
                .Setup(x => x.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>()))
                .ReturnsAsync(_fixture.Create<GetShortCoursesByAcademicYearResponse>());

            _mockPagedLinkHeaderService
                .Setup(x => x.GetPageLinks(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>(),
                    It.IsAny<GetShortCoursesByAcademicYearResponse>()))
                .Returns(new KeyValuePair<string, StringValues>("Link", StringValues.Empty));

            // Act
            await _sut.GetByAcademicYear(ukprn, academicYear, page, pageSize);

            // Assert
            _mockQueryDispatcher.Verify(x =>
                x.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(
                    It.Is<GetShortCoursesByAcademicYearRequest>(r =>
                        r.UkPrn == ukprn &&
                        r.AcademicYear == academicYear &&
                        r.Page == page &&
                        r.PageSize == pageSize)),
                Times.Once);
        }

        [Test]
        public async Task ThenPageSizeIsClampedToMax100()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            var academicYear = 2425;

            _mockQueryDispatcher
                .Setup(x => x.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>()))
                .ReturnsAsync(_fixture.Create<GetShortCoursesByAcademicYearResponse>());

            _mockPagedLinkHeaderService
                .Setup(x => x.GetPageLinks(
                    It.IsAny<GetShortCoursesByAcademicYearRequest>(),
                    It.IsAny<GetShortCoursesByAcademicYearResponse>()))
                .Returns(new KeyValuePair<string, StringValues>("Link", StringValues.Empty));

            // Act
            await _sut.GetByAcademicYear(ukprn, academicYear, 1, 500);

            // Assert
            _mockQueryDispatcher.Verify(x =>
                x.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(
                    It.Is<GetShortCoursesByAcademicYearRequest>(r => r.PageSize == 100)),
                Times.Once);
        }
    }
}
