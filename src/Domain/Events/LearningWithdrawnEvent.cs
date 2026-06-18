namespace SFA.DAS.Learning.Domain.Events;

public class LearningWithdrawnEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public short WithdrawnReasonCode { get; set; }
    public DateTime Created { get; set; }
    public long EmployerAccountId { get; set; }
}