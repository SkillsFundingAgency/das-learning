namespace SFA.DAS.Learning.Domain.Events;

public class LearningWithdrawnEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public long EmployerAccountId { get; set; }
}