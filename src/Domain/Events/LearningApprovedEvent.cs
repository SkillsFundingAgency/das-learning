namespace SFA.DAS.Learning.Domain.Events
{
    public class LearningApprovedEvent : IDomainEvent
    {
        public Guid LearningKey { get; set; }
        public Guid EpisodeKey { get; set; }
        public long EmployerAccountId { get; set; }
        public long FundingAccountId { get; set; }
    }
}
