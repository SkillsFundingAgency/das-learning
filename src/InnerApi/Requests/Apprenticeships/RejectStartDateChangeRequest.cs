namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

/// <summary>
/// Request to reject a pending start date change request
/// </summary>
public class RejectStartDateChangeRequest
{
    /// <summary>
    /// The reason for rejecting the start date change
    /// </summary>
    public string? Reason { get; set; }
}