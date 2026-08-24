using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;
using SFA.DAS.Learning.Queries.GetLearnings;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

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
    /// Creates a draft apprenticeship
    /// </summary>
    /// <param name="ukprn">UK provider reference number. Present in the route for future requirements; currently unused.</param>
    /// <param name="request">Details of learning</param>
    /// <returns>An array of <see cref="CreateDraftApprenticeshipLearningCommandResult"/> Object containing the result of the draft creation.</returns>
    [HttpPost("{ukprn}/apprenticeships")]
    [ProducesResponseType(200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CreateDraftLearning(long ukprn, [FromBody] CreateDraftApprenticeship request)
    {
        _logger.LogInformation("Creating learning with ukprn {ukprn} uln {uln}", ukprn, request.Learner.Uln);

        var command = new CreateDraftApprenticeshipLearningCommand(ukprn, request.ToUpdateModel(), request.Delivery.TrainingCode, request.AcademicYear);

        var result = await _commandDispatcher.Send<CreateDraftApprenticeshipLearningCommand, CreateDraftApprenticeshipLearningCommandResult>(command);

        return new OkObjectResult(result);
    }


    /// <summary>
    /// Checks whether an apprenticeship record already exists for the given ULN, training code and start date.
    /// </summary>
    /// <param name="ukprn">UK provider reference number.</param>
    /// <param name="uln">Unique learner number.</param>
    /// <param name="trainingCode">Training code (standard code) of the apprenticeship.</param>
    /// <param name="startDate">Start date of the apprenticeship. Only month and year are used for matching.</param>
    /// <param name="isApproved">Approval status to match against.</param>
    /// <response code="200">A matching apprenticeship record exists</response>
    /// <response code="404">No matching apprenticeship record exists</response>
    [HttpHead("{ukprn}/apprenticeships")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckApprovedApprenticeshipExists(long ukprn, string uln, string trainingCode, DateTime startDate, bool isApproved)
    {
        var request = new CheckApprovedApprenticeshipExistsRequest(ukprn, uln, trainingCode, startDate, isApproved);
        var response = await _queryDispatcher.Send<CheckApprovedApprenticeshipExistsRequest, CheckApprovedApprenticeshipExistsResponse>(request);

        return response.Exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Get learnings
    /// </summary>
    /// <param name="ukprn">Filter by training provider using the unique provider number.</param>
    /// <param name="fundingPlatform" >Filter by the funding platform. This parameter is optional.</param>
    /// <remarks>Gets all apprenticeships. The response from this endpoint only contains summary apprenticeship information.</remarks>
    /// <response code="200">Apprenticeships retrieved</response>
    [HttpGet("{ukprn}/learnings")]
    [ProducesResponseType(typeof(IEnumerable<LearnerSummary>), 200)]
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
    /// Updates the details of a learner associated with a specific learner key.
    /// </summary>
    /// <param name="ukprn">UK provider reference number.</param>
    /// <param name="learnerKey">The unique identifier for the learner record to update.</param>
    /// <param name="request">The updated learner details.</param>
    /// <returns>An array of <see cref="LearningUpdateChanges"/> values indicating the fields that were modified.</returns>
    [HttpPut("{ukprn}/{learnerKey}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UpdateLearning(long ukprn, Guid learnerKey, [FromBody] UpdateLearnerRequest request)
    {
        _logger.LogInformation("Updating learning for learner with key {LearnerKey}", learnerKey);

        var command = new UpdateLearnerCommand(learnerKey, ukprn, request.Delivery.TrainingCode, request.ToUpdateModel());

        var result = await _commandDispatcher.Send<UpdateLearnerCommand, UpdateLearnerResult>(command);

        return new OkObjectResult(result);
    }

    /// <summary>
    /// Removes a learner associated with a specific learner key.
    /// </summary>
    /// <param name="ukprn">UK provider reference number. Scopes removal to learnings at this provider.</param>
    /// <param name="learnerKey">The unique identifier for the learner record to remove.</param>
    /// <param name="academicYear">Academic year in yyyy format (e.g. 2425). Scopes removal to learnings overlapping this academic year.</param>
    /// <returns>The learning keys that were removed for the learner.</returns>
    [HttpDelete("{ukprn}/{learnerKey}")]
    [ProducesResponseType(typeof(List<Guid>), 200)]
    public async Task<IActionResult> RemoveLearning(long ukprn, Guid learnerKey, [FromQuery] int academicYear)
    {
        _logger.LogInformation("Deleting learner with key {LearnerKey}", learnerKey);

        var command = new RemoveLearnerCommand(learnerKey, ukprn, academicYear);

        var removedLearningKeys = await _commandDispatcher.Send<RemoveLearnerCommand, List<Guid>>(command);

        return new OkObjectResult(removedLearningKeys);
    }
}