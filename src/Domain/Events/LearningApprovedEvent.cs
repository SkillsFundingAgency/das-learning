namespace SFA.DAS.Learning.Domain.Events
{
    public class LearningApprovedEvent : IDomainEvent
    {
        public Guid LearningKey { get; set; }
    }
}
