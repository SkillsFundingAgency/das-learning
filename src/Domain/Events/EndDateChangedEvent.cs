namespace SFA.DAS.Learning.Domain.Events
{
    public class EndDateChangedEvent : IDomainEvent
    {
        public Guid LearningKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
}
