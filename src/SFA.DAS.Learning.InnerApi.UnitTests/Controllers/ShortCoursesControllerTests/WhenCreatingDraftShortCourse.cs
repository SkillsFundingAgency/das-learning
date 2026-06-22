using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.InnerApi.Controllers;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using System.Linq;

namespace SFA.DAS.Learning.InnerApi.UnitTests.Controllers.ShortCoursesControllerTests;

public class WhenCreatingDraftShortCourse
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
    }

    [Test]
    public async Task ThenReturnsLearningKey()
    {
        // Arrange
        var request = _fixture.Create<CreateDraftShortCourseRequest>();
        var expectedLearningKey = _fixture.Create<Guid>();
        var expectedEpisodeKey = _fixture.Create<Guid>();
        var isReinstated = _fixture.Create<bool>();
        var commandResult = new CreateDraftShortCourseCommandResult { LearningKey = expectedLearningKey, EpisodeKey = expectedEpisodeKey, IsReinstated = isReinstated };

        _mockCommandDispatcher
            .Setup(x => x.Send<CreateDraftShortCourseCommand, CreateDraftShortCourseCommandResponse>(
                It.IsAny<CreateDraftShortCourseCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateDraftShortCourseCommandResponse { Results = [commandResult] });

        // Act
        var result = await _sut.CreateDraftShortCourse(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(new CreateDraftShortCourseCommandResponse { Results = [new() { LearningKey = expectedLearningKey, EpisodeKey = expectedEpisodeKey, IsReinstated = isReinstated }] });

        _mockCommandDispatcher.Verify(x =>
            x.Send<CreateDraftShortCourseCommand, CreateDraftShortCourseCommandResponse>(
                It.Is<CreateDraftShortCourseCommand>(c =>
                    c.Models.Count == request.OnProgramme.Count &&
                    c.Models.All(m => m.Learner.EmailAddress == request.LearnerUpdateDetails.EmailAddress) &&
                    c.Models.Select(m => m.OnProgramme.CourseCode).SequenceEqual(request.OnProgramme.Select(op => op.CourseCode))
                ),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
