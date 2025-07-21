using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Identity.Authorization;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetLearnings;
using SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;
using SFA.DAS.Learning.Queries.GetApprenticeshipStartDate;
using SFA.DAS.Learning.Queries.GetCurrentPartyIds;
using SFA.DAS.Learning.Queries.GetLearnerStatus;
using SFA.DAS.Learning.Queries.GetLearningKey;
using SFA.DAS.Learning.Queries.GetLearningKeyByLearningId;
using SFA.DAS.Learning.Queries.GetLearningPrice;
using SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Command.UpdateLearner;

namespace SFA.DAS.Learning.InnerApi.Controllers;

/// <summary>
/// Controller for handling requests related to Apprenticeships
/// </summary>
[Route("")]
[ApiController]
[ControllerAuthorizeUserType(UserType.Provider | UserType.Employer)]
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
    [ProducesResponseType(typeof(IEnumerable<DataTransferObjects.Learning>), 200)]
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
    [ActionAuthorizeUserType(UserType.ServiceAccount)]
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
    /// Get Learning Price
    /// </summary>
    /// <param name="learningKey"></param>
    /// <returns>Learning Price</returns>
    [HttpGet("{learningKey}/price")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearningPrice(Guid learningKey)
    {
        var request = new GetLearningPriceRequest { ApprenticeshipKey = learningKey };
        var response = await _queryDispatcher.Send<GetLearningPriceRequest, GetLearningPriceResponse?>(request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    /// Get Learning Start Date
    /// </summary>
    /// <param name="learningKey">Guid</param>
    /// <returns>Learning Start Date or NotFound</returns>
    [HttpGet("{learningKey}/startDate")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetStartDate(Guid learningKey)
    {
        var request = new GetLearningStartDateRequest { ApprenticeshipKey = learningKey };
        var response = await _queryDispatcher.Send<GetLearningStartDateRequest, GetLearningStartDateResponse?>(request);
        if (response == null || response.LearningStartDate == null) return NotFound();
        return Ok(response.LearningStartDate);
    }

    /// <summary>
    /// Get Learning Key
    /// </summary>
    /// <param name="apprenticeshipHashedId">This should be the hashed id for the apprenticeship not the commitment</param>
    /// <returns>Learning Key</returns>
    [HttpGet("{apprenticeshipHashedId}/key")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearningKey(string apprenticeshipHashedId)
    {
        var request = new GetLearningKeyRequest { ApprenticeshipHashedId = apprenticeshipHashedId };
        var response = await _queryDispatcher.Send<GetLearningKeyRequest, GetLearningKeyResponse>(request);
        if (response.LearningKey == null) return NotFound();
        return Ok(response.LearningKey);
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
    /// Gets the Learner Status of the learning
    /// </summary>
    /// <param name="learningKey">Guid</param>
    /// <returns>GetLearnerStatusResponse containing LearnerStatus</returns>
    [HttpGet("{learningKey}/LearnerStatus")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearnerStatus(Guid learningKey)
    {
        var request = new GetLearnerStatusRequest { ApprenticeshipKey = learningKey };
        var response = await _queryDispatcher.Send<GetLearnerStatusRequest, GetLearnerStatusResponse?>(request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    /// Gets all learnings for a given provider with episode & price data
    /// </summary>
    /// <param name="ukprn">Ukprn</param>
    /// <param name="collectionYear">Collection Year</param>
    /// <param name="collectionPeriod">Collection Period</param>
    /// <returns>GetLearningsWithEpisodesResponse containing learning, episode, & price data</returns>
    [HttpGet("{ukprn}/{collectionYear}/{collectionPeriod}")]
    [ProducesResponseType(200)]
    [ActionAuthorizeUserType(UserType.ServiceAccount)]
    public async Task<IActionResult> GetLearningsForFm36(long ukprn, short collectionYear, byte collectionPeriod)
    {
        var request = new GetLearningsWithEpisodesRequest { Ukprn = ukprn, CollectionYear = collectionYear, CollectionPeriod = collectionPeriod };
        var response = await _queryDispatcher.Send<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>(request);
        if (response == null) return NotFound();
        return Ok(response);
    }


    /// <summary>
    /// Get the provider and employer ids for the current state of the learning, this may differ from the original owner
    /// </summary>
    /// <param name="learningKey">Guid</param>
    /// <returns>Provider and employer ids</returns>
    [HttpGet("{learningKey}/currentPartyIds")]
    [ProducesResponseType(200)]
    [ActionAuthorizeUserType(UserType.ServiceAccount | UserType.Provider | UserType.Employer)]
    public async Task<IActionResult> GetCurrentPartyIds(Guid learningKey)
    {
        var request = new GetCurrentPartyIdsRequest { ApprenticeshipKey = learningKey };
        var response = await _queryDispatcher.Send<GetCurrentPartyIdsRequest, GetCurrentPartyIdsResponse?>(request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    /// Updates the details of a learner associated with a specific learning key.
    /// </summary>
    /// <param name="learningKey">The unique identifier for the learner record to update.</param>
    /// <param name="request">The updated learner details.</param>
    /// <returns>An array of <see cref="LearningUpdateChanges"/> values indicating the fields that were modified.</returns>
    [HttpPut("{learningKey}")]
    [ProducesResponseType(200)]
    [ActionAuthorizeUserType(UserType.ServiceAccount)]
    public async Task<IActionResult> UpdateLearning(Guid learningKey, [FromBody] UpdateLearnerRequest request)
    {
        _logger.LogInformation("Updating learning with key {LearningKey}", learningKey);

        var command = request.ToCommand(learningKey);

        var result = await _commandDispatcher.Send<UpdateLearnerCommand, LearningUpdateChanges[]>(command);

        return new OkObjectResult(result);
    }
}