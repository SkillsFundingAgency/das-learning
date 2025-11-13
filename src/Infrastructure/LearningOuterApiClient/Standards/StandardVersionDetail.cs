using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

[ExcludeFromCodeCoverage]
public class StandardVersionDetail
{
    public DateTime? EarliestStartDate { get; set; }
    public DateTime? LatestStartDate { get; set; }
    public DateTime? LatestEndDate { get; set; }
    public DateTime? ApprovedForDelivery { get; set; }
    public int ProposedTypicalDuration { get; set; }
    public int ProposedMaxFunding { get; set; }
}