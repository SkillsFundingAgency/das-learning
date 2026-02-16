using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetLearnings;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;
using SFA.DAS.Learning.Queries.GetLearningKeyByLearningId;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

namespace SFA.DAS.Learning.InnerApi.Controllers;

///<summary>
/// Controller for handling learning for full apprenticeships
///</summary>
[Route("")]
[ApiController]
public class LearningController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<LearningController> _logger;
    private readonly IPagedLinkHeaderService _pagedLinkHeaderService;

    /// <summary>Initializes a new instance of the <see cref="LearningController"/> class.</summary>
    /// <param name="queryDispatcher">Gets data</param>
    /// <param name="commandDispatcher">updates data</param>
    /// <param name="logger">ILogger</param>
    /// <param name="pagedLinkHeaderService">IPagedQueryResultHelper</param>
    public LearningController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ILogger<LearningController> logger, IPagedLinkHeaderService pagedLinkHeaderService)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
        _pagedLinkHeaderService = pagedLinkHeaderService;
    }

    /// <summary>
    /// Get learnings
    /// </summary>
    /// <param name="ukprn">Filter by training provider using the unique provider number.</param>
    /// <param name="fundingPlatform" >Filter by the funding platform. This parameter is optional.</param>
    /// <remarks>Gets all apprenticeships. The response from this endpoint only contains summary apprenticeship information.</remarks>
    /// <response code="200">Apprenticeships retrieved</response>
    [HttpGet("{ukprn}/learnings")]
    [ProducesResponseType(typeof(IEnumerable<Models.Dtos.Learning>), 200)]
    public async Task<IActionResult> GetAll(long ukprn, FundingPlatform? fundingPlatform)
    {
        var request = new GetLearningsRequest(ukprn, fundingPlatform);
        var response = await _queryDispatcher.Send<GetLearningsRequest, GetLearningsResponse>(request);

        return Ok(response.Learnings);
    }   

    /// <summary>
    /// Get paginated learnings for a provider between specified dates.
    /// </summary>
    /// <param name="ukprn">UkPrn filter value</param>
    /// <param name="academicYear">Academic year in yyyy format (e.g. 2425)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>GetLearningsByAcademicYearResponse</returns>
    [HttpGet("{ukprn:long}/academicyears/{academicYear:int}/learnings")]
    [ProducesResponseType(typeof(GetLearningsByAcademicYearResponse), 200)]
    public async Task<IActionResult> GetByAcademicYear(long ukprn, int academicYear, [FromQuery] int page = 1, [FromQuery] int? pageSize = 20)
    {
        pageSize = pageSize.HasValue ? Math.Clamp(pageSize.Value, 1, 100) : pageSize;
        
        var request = new GetLearningsByAcademicYearRequest(ukprn, academicYear, page, pageSize);
        var response = await _queryDispatcher.Send<GetLearningsByAcademicYearRequest, GetLearningsByAcademicYearResponse>(request);

        var pageLinks = _pagedLinkHeaderService.GetPageLinks(request, response);
        
        Response?.Headers.Add(pageLinks);

        return Ok(response);
    }

    /// <summary>
    /// Get Learning Key
    /// </summary>
    /// <param name="learningId">This should be the id for the learning not the commitment</param>
    /// <returns>Learning Key</returns>
    [HttpGet("{learningId}/key2")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearningKeyByLearningId(long learningId)
    {
        var request = new GetLearningKeyByLearningIdRequest { ApprenticeshipId = learningId };
        var response = await _queryDispatcher.Send<GetLearningKeyByLearningIdRequest, GetLearningKeyByLearningIdResponse>(request);
        if (response.LearningKey == null)
        {
            _logger.LogInformation("{p1} could not be found.", nameof(response.LearningKey));
            return NotFound();
        }

        return Ok(response.LearningKey);
    }

    /// <summary>
    /// Gets all fm36 learnings data for a given provider with episode and price data
    /// </summary>
    /// <param name="ukprn">Ukprn</param>
    /// <param name="collectionYear">Collection Year</param>
    /// <param name="collectionPeriod">Collection Period</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>GetLearningsWithEpisodesResponse containing learning, episode, and price data</returns>
    [HttpGet("{ukprn}/{collectionYear}/{collectionPeriod}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearningsForFm36(long ukprn, short collectionYear, byte collectionPeriod, [FromQuery] int? page = null, [FromQuery] int? pageSize = null)
    {
        var request = new GetLearningsWithEpisodesRequest { Ukprn = ukprn, CollectionYear = collectionYear, CollectionPeriod = collectionPeriod, Page = page ?? -1, PageSize = pageSize};
        var response = await _queryDispatcher.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(request);
        if (response == null) return NotFound();

        if (page != null && pageSize != null)
            return Ok(response);
        else
            return Ok(response.Items);
    }

    /// <summary>
    /// Updates the details of a learner associated with a specific learning key.
    /// </summary>
    /// <param name="learningKey">The unique identifier for the learner record to update.</param>
    /// <param name="request">The updated learner details.</param>
    /// <returns>An array of <see cref="LearningUpdateChanges"/> values indicating the fields that were modified.</returns>
    [HttpPut("{learningKey}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UpdateLearning(Guid learningKey, [FromBody] UpdateLearnerRequest request)
    {
        _logger.LogInformation("Updating learning with key {LearningKey}", learningKey);

        var command = new UpdateLearnerCommand(learningKey, request.ToUpdateModel());

        var result = await _commandDispatcher.Send<UpdateLearnerCommand, UpdateLearnerResult>(command);

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Removes a learner associated with a specific learning key.
    /// </summary>
    /// <param name="ukprn">UK provider reference number. Present in the route for future requirements; currently unused.</param>
    /// <param name="learningKey">The unique identifier for the learner record to remove.</param> -->
    [HttpDelete("{ukprn}/{learningKey}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RemoveLearning(long ukprn, Guid learningKey)
    {
        _logger.LogInformation("Deleting learning with key {LearningKey}", learningKey);

        var command = new RemoveLearnerCommand(learningKey);

        var result = await _commandDispatcher.Send<RemoveLearnerCommand, RemoveLearnerResult>(command);

        return new OkObjectResult(result);
    }
}