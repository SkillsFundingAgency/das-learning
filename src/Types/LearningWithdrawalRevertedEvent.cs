namespace SFA.DAS.Learning.Types;

public class LearningWithdrawalRevertedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
}