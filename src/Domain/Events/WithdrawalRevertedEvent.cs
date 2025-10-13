namespace SFA.DAS.Learning.Domain.Events;

public class WithdrawalRevertedEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
}