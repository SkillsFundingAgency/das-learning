using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class ApprenticeshipOptInConfiguration
{
    public DateTime StartDate { get; set; }
    public List<long> PaymentsOptedInProviders { get; set; } = [];
}
