namespace SFA.DAS.Learning.Types
{
    public class EndDateChangedEvent
    {
        public Guid LearningKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
}
