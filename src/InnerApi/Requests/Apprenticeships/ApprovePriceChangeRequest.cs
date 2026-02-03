namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

/// <summary>
/// Request to approve a pending price change request
/// </summary>
public class ApprovePriceChangeRequest
{
    /// <summary>
    /// Id of the approver, either the provider or the employer
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The training price set by the provider for employer-initiated price changes
    /// </summary>
    public decimal? TrainingPrice { get; set; }

    /// <summary>
    /// The assessment price set by the provider for employer-initiated price changes
    /// </summary>
    public decimal? AssessmentPrice { get; set; }
}