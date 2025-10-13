namespace SFA.DAS.Learning.Types;

public class WithdrawalRevertedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
}