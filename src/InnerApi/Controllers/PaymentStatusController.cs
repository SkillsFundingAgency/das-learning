using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.SetPaymentsFrozen;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetApprenticeshipPaymentStatus;

namespace SFA.DAS.Learning.InnerApi.Controllers;

/// <summary>
/// Controller for handling requests related to Learning Payment Status
/// </summary>
[Route("{learningKey}")]
[ApiController]
public class PaymentStatusController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<PaymentStatusController> _logger;

    /// <summary>Initializes a new instance of the <see cref="PaymentStatusController"/> class.</summary>
    /// <param name="queryDispatcher">Gets data</param>
    /// <param name="commandDispatcher">updates data</param>
    /// <param name="logger">ILogger</param>
    public PaymentStatusController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ILogger<PaymentStatusController> logger)
    {
        _queryDispatcher = queryDispatcher;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Get Learning Payment Status (Frozen/Unfrozen)
    /// </summary>
    /// <param name="learningKey"></param>
    /// <returns>Learning Payment Status (Frozen/Unfrozen)</returns>
    [HttpGet("paymentStatus")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetLearningPaymentStatus(Guid learningKey)
    {
        var request = new GetLearningPaymentStatusRequest { LearningKey = learningKey };
        var response = await _queryDispatcher.Send<GetLearningPaymentStatusRequest, GetLearningPaymentStatusResponse?>(request);
        if (response == null) return NotFound();
        return Ok(response);
    }

    /// <summary>
    /// Changes Learning Payment Status to Frozen
    /// </summary>
    /// <param name="learningKey"></param>
    /// <param name="freezeRequest"></param>
    [HttpPost("freeze")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> FreezePaymentStatus(Guid learningKey, [FromBody] FreezeRequest freezeRequest)
    {
        try
        {
            await _commandDispatcher.Send(new SetPaymentsFrozenCommand(learningKey, "", SetPayments.Freeze, freezeRequest.Reason));
            return Ok();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{method} failed to update {learningKey} : {message}",
                nameof(FreezePaymentStatus), learningKey, exception.Message);
            return BadRequest();
        }
    }

    /// <summary>
    /// Changes Learning Payment Status to Active
    /// </summary>
    /// <param name="learningKey"></param>
    [HttpPost("unfreeze")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UnfreezePaymentStatus(Guid learningKey)
    {
        try
        {
            await _commandDispatcher.Send(new SetPaymentsFrozenCommand(learningKey, "", SetPayments.Unfreeze));
            return Ok();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{method} failed to update {learningKey} : {message}",
                nameof(UnfreezePaymentStatus), learningKey, exception.Message);
            return BadRequest();
        }
    }
}
