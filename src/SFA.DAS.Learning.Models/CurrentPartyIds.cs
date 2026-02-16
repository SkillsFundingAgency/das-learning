using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Models;

[ExcludeFromCodeCoverage]
public class CurrentPartyIds
{
    public CurrentPartyIds(long ukprn, long employerAccountId, long approvalsApprenticeshipId)
    {
        Ukprn = ukprn;
        EmployerAccountId = employerAccountId;
        ApprovalsApprenticeshipId = approvalsApprenticeshipId;
    }

    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
}