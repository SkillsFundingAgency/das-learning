namespace SFA.DAS.Learning.Domain.Events;

public class LearningRemovedEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
}
