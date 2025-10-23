using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Domain;

[ExcludeFromCodeCoverage]
public class LearnerStatusDetails
{
    public DateTime? WithdrawalChangedDate { get; set; }
    public string? WithdrawalReason { get; set; }
    public DateTime? LastDayOfLearning { get; set; }
}