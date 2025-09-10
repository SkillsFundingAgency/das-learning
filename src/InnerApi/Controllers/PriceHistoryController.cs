using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.ApprovePriceChange;
using SFA.DAS.Learning.Command.CancelPendingPriceChange;
using SFA.DAS.Learning.Command.CreatePriceChange;
using SFA.DAS.Learning.Command.RejectPendingPriceChange;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests;
using SFA.DAS.Learning.InnerApi.Responses;
using SFA.DAS.Learning.Queries;
using SFA.DAS.Learning.Queries.GetPendingPriceChange;

namespace SFA.DAS.Learning.InnerApi.Controllers
{
    /// <summary>
    /// Controller for handling requests related to price history
    /// </summary>
    [Route("")]
    [ApiController]
    public class PriceHistoryController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<PriceHistoryController> _logger;

        /// <summary>Initializes a new instance of the <see cref="PriceHistoryController"/> class.</summary>
        /// <param name="queryDispatcher"></param>
        /// <param name="commandDispatcher"></param>
        /// <param name="logger"></param>
        public PriceHistoryController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, ILogger<PriceHistoryController> logger)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        /// <summary>
        /// Create apprenticeship price change
        /// </summary>
        /// <param name="learningKey">The unique identifier of the learning</param>
        /// <param name="request">Details of the requested price change.</param>
        /// <returns>Ok on success, Bad Request if neither employer or provider are set for initiator</returns>
        [HttpPost("{learningKey}/priceHistory")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateLearningPriceChange(Guid learningKey, [FromBody] PostCreateLearningPriceChangeRequest request)
        {
            try
            {
                var priceChangeStatus = await _commandDispatcher.Send<CreatePriceChangeCommand, ChangeRequestStatus>(new CreatePriceChangeCommand(request.Initiator, learningKey, request.UserId, request.TrainingPrice, request.AssessmentPrice, request.TotalPrice, request.Reason, request.EffectiveFromDate));
                return Ok(new CreatePriceChangeResponse { PriceChangeStatus = priceChangeStatus.ToString()});
            }
            catch (ArgumentException exception)
            {
                _logger.LogError(exception, $"Bad Request, missing or invalid: {exception.ParamName}");
                return BadRequest();
            }
        }

        /// <summary>
        /// Gets the details of a pending price change
        /// </summary>
        /// <param name="learningKey">The unique identifier of the learning</param>
        /// <returns>Details of the pending price change</returns>
        [HttpGet("{learningKey}/priceHistory/pending")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetPendingPriceChange(Guid learningKey)
        {
            var request = new GetPendingPriceChangeRequest(learningKey);
            var response = await _queryDispatcher.Send<GetPendingPriceChangeRequest, GetPendingPriceChangeResponse>(request);

            if(!response.HasPendingPriceChange)
            {
                return NotFound(response.PendingPriceChange);
            }

            return Ok(response);
        }

        /// <summary>
        /// Approves a pending price change
        /// </summary>
        /// <param name="learningKey">The unique identifier of the learning</param>
        /// <param name="request">Details of the approval</param>
        [HttpPatch("{learningKey}/priceHistory/pending")]
		[ProducesResponseType(200)]
		public async Task<IActionResult> ApprovePriceChange(Guid learningKey, [FromBody] ApprovePriceChangeRequest request)
		{
			var approver = await _commandDispatcher.Send<ApprovePriceChangeCommand, ApprovedBy>(new ApprovePriceChangeCommand(learningKey, request.UserId, request.TrainingPrice, request.AssessmentPrice));
            return Ok(new { Approver = approver.ToString() });
        }

		/// <summary>
		/// Removes a pending price change
		/// </summary>
		/// <param name="learningKey">The unique identifier of the learning</param>
		/// <returns>Ok result</returns>
		[HttpDelete("{learningKey}/priceHistory/pending")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CancelPendingPriceChange(Guid learningKey)
        {
            var request = new CancelPendingPriceChangeRequest(learningKey);
            await _commandDispatcher.Send(request);

            return Ok();
        }

        /// <summary>
        /// Rejects a pending price change
        /// </summary>
        /// <param name="learningKey">The unique identifier of the learning</param>
        /// <param name="request">Details of the rejection</param>
        /// <returns>ok result</returns>
        [HttpPatch("{learningKey}/priceHistory/pending/reject")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RejectPendingPriceChange(Guid learningKey, [FromBody]PatchRejectPriceChangeRequest request)
        {
            await _commandDispatcher.Send(new RejectPendingPriceChangeRequest(learningKey, request.Reason));

            return Ok(new { Rejector = "" });
        }
    }
}
