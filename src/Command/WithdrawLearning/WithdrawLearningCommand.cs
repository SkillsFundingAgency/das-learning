#pragma warning disable 8618
using SFA.DAS.Learning.Domain.Validators;

namespace SFA.DAS.Learning.Command.WithdrawLearning;

public class WithdrawLearningCommand : ICommand
{
    public long UKPRN { get; set; }
    public string ULN { get; set; }
    public string Reason { get; set; }
    public string ReasonText { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public string ProviderApprovedBy { get; set; }
}

public static class WithdrawLearningCommandExtensions
{
    public static WithdrawDomainRequest ToDomainRequest(this WithdrawLearningCommand command)
    {
        return new WithdrawDomainRequest
        {
            UKPRN = command.UKPRN,
            ULN = command.ULN,
            Reason = command.Reason,
            ReasonText = command.ReasonText,
            LastDayOfLearning = command.LastDayOfLearning,
            ProviderApprovedBy = command.ProviderApprovedBy
        };
    }
}