using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Command.DeleteShortCourse;
using SFA.DAS.Learning.Command.UpdateShortCourse;
using SFA.DAS.Learning.InnerApi.Requests.ShortCourses;
using SFA.DAS.Learning.InnerApi.Responses;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;
using SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

namespace SFA.DAS.Learning.InnerApi.Controllers;

/// <summary>
/// Controller for managing short courses.
/// </summary>
[Route("")]
[ApiController]
public class ShortCoursesController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<ShortCoursesController> _logger;
    private readonly IPagedLinkHeaderService _pagedLinkHeaderService;

    /// <summary>Initializes a new instance of the <see cref="ShortCoursesController"/> class.</summary>
    /// <param name="queryDispatcher">Gets data</param>
    /// <param name="commandDispatcher">updates data</param>
    /// <param name="logger">ILogger</param>
    /// <param name="pagedLinkHeaderService">IPagedQueryResultHelper</param>
    public ShortCoursesController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ILogger<ShortCoursesController> logger, IPagedLinkHeaderService pagedLinkHeaderService)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
        _pagedLinkHeaderService = pagedLinkHeaderService;
    }

    /// <summary>
    /// Get paginated short courses for a provider within an academic year.
    /// </summary>
    /// <param name="ukprn">Ukprn</param>
    /// <param name="academicYear">Academic year in yyyy format (e.g. 2425)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>GetShortCoursesByAcademicYearResponse</returns>
    [HttpGet("{ukprn:long}/academicyears/{academicYear:int}/shortCourses")]
    [ProducesResponseType(typeof(GetShortCoursesByAcademicYearResponse), 200)]
    public async Task<IActionResult> GetByAcademicYear(long ukprn, int academicYear, [FromQuery] int page = 1, [FromQuery] int? pageSize = 20)
    {
        pageSize = pageSize.HasValue ? Math.Clamp(pageSize.Value, 1, 100) : pageSize;

        var request = new GetShortCoursesByAcademicYearRequest(ukprn, academicYear, page, pageSize);
        var response = await _queryDispatcher.Send<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>(request);

        var pageLinks = _pagedLinkHeaderService.GetPageLinks(request, response);

        Response?.Headers.Add(pageLinks);

        return Ok(response);
    }

    /// <summary>
    /// Get paginated short courses for a provider within a collection year, for earnings purposes.
    /// </summary>
    /// <param name="ukprn">Ukprn</param>
    /// <param name="collectionYear">Collection year in yymm format (e.g. 2425)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>GetShortCoursesForEarningsResponse</returns>
    [HttpGet("{ukprn:long}/{collectionYear:int}/shortCourses")]
    [ProducesResponseType(typeof(GetShortCoursesForEarningsResponse), 200)]
    public async Task<IActionResult> GetForEarnings(long ukprn, int collectionYear, [FromQuery] int page = 1, [FromQuery] int? pageSize = 20)
    {
        pageSize = pageSize.HasValue ? Math.Clamp(pageSize.Value, 1, 100) : pageSize;

        var request = new GetShortCoursesForEarningsRequest(ukprn, collectionYear, page, pageSize);
        var response = await _queryDispatcher.Send<GetShortCoursesForEarningsRequest, GetShortCoursesForEarningsResponse>(request);

        var pageLinks = _pagedLinkHeaderService.GetPageLinks(request, response);

        Response?.Headers.Add(pageLinks);

        return Ok(response);
    }

    /// <summary>
    /// Creates a new draft (unapproved) short course learner.
    /// </summary>
    /// <param name="request">The learner and short course details.</param>
    /// <returns>The newly created LearningKey.</returns>
    [HttpPost("shortCourses")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CreateDraftShortCourse([FromBody] CreateDraftShortCourseRequest request)
    {
        _logger.LogInformation("Creating draft short course (ukprn: {ukprn})", request.OnProgramme.Ukprn);

        var command = new CreateDraftShortCourseCommand(request.ToCreateModel());

        var result =
            await _commandDispatcher.Send<CreateDraftShortCourseCommand, CreateDraftShortCourseCommandResult>(command);

        if (result.LearningKey == null)
            return NoContent();

        return new OkObjectResult(new CreateShortCourseLearningResponse { LearningKey = result.LearningKey.Value, EpisodeKey = result.EpisodeKey!.Value });
    }

    /// <summary>
    /// Deletes a short course learner record.
    /// </summary>
    /// <param name="ukprn">The ukprn of the provider in context</param>
    /// <param name="learningKey">The key of the short course learning record to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{ukprn:long}/shortCourses/{learningKey}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DeleteShortCourse(long ukprn, Guid learningKey)
    {
        var result = await _commandDispatcher.Send<DeleteShortCourseCommand, DeleteShortCourseResult>(new DeleteShortCourseCommand(learningKey, ukprn));
        return result.WasDeleted ? NoContent() : Ok();
    }

    /// <summary>
    /// Updates an existing short course learner record.
    /// </summary>
    /// <param name="learningKey">The key of the short course learning record to update.</param>
    /// <param name="request">The updated learner and short course details.</param>
    /// <returns>The LearningKey and list of fields that changed.</returns>
    [HttpPut("shortCourses/{learningKey}")]
    [ProducesResponseType(typeof(UpdateShortCourseResult), 200)]
    public async Task<IActionResult> UpdateShortCourse(Guid learningKey, [FromBody] CreateDraftShortCourseRequest request)
    {
        var command = new UpdateShortCourseCommand(learningKey, request.ToCreateModel());
        var result = await _commandDispatcher.Send<UpdateShortCourseCommand, UpdateShortCourseResult>(command);
        return new OkObjectResult(result);
    }
}