namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

/// <summary>
/// Request model for rejecting a price change.
/// </summary>
public class PatchRejectPriceChangeRequest
{
    /// <summary>
    /// The reason for rejecting the price change
    /// </summary>
    public string? Reason { get; set; }
}