using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ApproveStartDateChange;
using SFA.DAS.Learning.Command.CancelPendingStartDateChange;
using SFA.DAS.Learning.Command.CreateStartDateChange;
using SFA.DAS.Learning.Command.RejectStartDateChange;
using SFA.DAS.Learning.InnerApi.Identity.Authorization;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetPendingStartDateChange;

namespace SFA.DAS.Learning.InnerApi.Controllers;

/// <summary>
/// Controller for handling requests related to start date changes
/// </summary>
[Route("")]
[ApiController]
public class StartDateChangeController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<StartDateChangeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartDateChangeController"/> class.
    /// </summary>
    /// <param name="queryDispatcher"></param>
    /// <param name="commandDispatcher"></param>
    /// <param name="logger"></param>
    public StartDateChangeController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ILogger<StartDateChangeController> logger)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Create learning start date change
    /// </summary>
    /// <param name="learningKey">The unique identifier of the learning</param>
    /// <param name="request">Details of the requested start date change.</param>
    /// <returns>Ok on success, Bad Request if neither employer or provider are set for initiator</returns>
    [HttpPost("{learningKey}/startDateChange")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CreateApprenticeshipStartDateChange(Guid learningKey, [FromBody] PostCreateApprenticeshipStartDateChangeRequest request)
    {
        try
        {
            await _commandDispatcher.Send(new CreateStartDateChangeCommand(request.Initiator, learningKey, request.UserId, request.ActualStartDate, request.PlannedEndDate, request.Reason));
            return Ok();
        }
        catch (ArgumentException exception)
        {
            _logger.LogError(exception, $"Bad Request, missing or invalid: {exception.ParamName}");
            return BadRequest();
        }
    }

    /// <summary>
    /// Gets the details of a pending start date change
    /// </summary>
    /// <param name="learningKey">The unique identifier of the learning</param>
    /// <returns>Details of the pending start date change</returns>
    [HttpGet("{learningKey}/startDateChange/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPendingStartDateChange(Guid learningKey)
    {
        var request = new GetPendingStartDateChangeRequest(learningKey);
        var response = await _queryDispatcher.Send<GetPendingStartDateChangeRequest, GetPendingStartDateChangeResponse>(request);

        if (!response.HasPendingStartDateChange)
        {
            return NotFound(response.PendingStartDateChange);
        }

        return Ok(response);
    }

    /// <summary>
    /// Approves a pending start date change
    /// </summary>
    /// <param name="learningKey">The unique identifier of the apprenticeship</param>
    /// <param name="request">Details of the request for start date change approval</param>
    [HttpPatch("{learningKey}/startDateChange/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ApproveStartDateChange(Guid learningKey, [FromBody] ApproveStartDateChangeRequest request)
    {
        await _commandDispatcher.Send(new ApproveStartDateChangeCommand(learningKey, request.UserId));
        return Ok();
    }

    /// <summary>
    /// Rejects a pending start date change
    /// </summary>
    /// <param name="learningKey">The unique identifier of the apprenticeship</param>
    /// <param name="request">Details of the request for start date change rejection</param>
    [HttpPatch("{learningKey}/startDateChange/reject")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RejectStartDateChange(Guid learningKey, [FromBody] RejectStartDateChangeRequest request)
    {
        await _commandDispatcher.Send(new RejectStartDateChangeCommand(learningKey, request.Reason));
        return Ok();
    }

    /// <summary>
    /// Removes a pending start date change
    /// </summary>
    /// <param name="learningKey">The unique identifier of the learning</param>
    /// <returns>Ok result</returns>
    [HttpDelete("{learningKey}/startDateChange/pending")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CancelPendingStartDateChange(Guid learningKey)
    {
	    var request = new CancelPendingStartDateChangeRequest(learningKey);
	    await _commandDispatcher.Send(request);

	    return Ok();
    }
}