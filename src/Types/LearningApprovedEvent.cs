namespace SFA.DAS.Learning.Types
{
    public class LearningApprovedEvent
    {
        public Guid LearningKey { get; set; }
        public Guid EpisodeKey { get; set; }
        public long EmployerAccountId { get; set; }
        public long FundingAccountId { get; set; }
    }
}
