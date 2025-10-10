namespace SFA.DAS.Learning.Types;

public class LearningWithdrawnEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public long EmployerAccountId { get; set; }
}