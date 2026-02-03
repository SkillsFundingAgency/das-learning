namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

#pragma warning disable CS8618

/// <summary>
/// Initial request for creating a new apprenticeship start date change, a separate request is required for approval
/// </summary>
public class PostCreateApprenticeshipStartDateChangeRequest
{
#pragma warning disable CS1591
    public string Initiator { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ActualStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Reason { get; set; } = null!;
#pragma warning restore CS1591
}

#pragma warning restore CS8618