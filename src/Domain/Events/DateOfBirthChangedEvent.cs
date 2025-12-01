namespace SFA.DAS.Learning.Domain.Events;

public class DateOfBirthChangedEvent : IDomainEvent
{
    public Guid LearningKey { get; set; }
    public DateTime DateOfBirth { get; set; }
}