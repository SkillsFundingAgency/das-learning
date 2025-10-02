namespace SFA.DAS.Learning.Domain.Events;

public class PersonalDetailsChangedEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? EmailAddress { get; set; } = null!;
}