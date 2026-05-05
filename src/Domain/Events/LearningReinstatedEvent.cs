namespace SFA.DAS.Learning.Domain.Events;

public class LearningReinstatedEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
}
