namespace SFA.DAS.Learning.Types;

public class LearningCreatedEvent : LearningEvent
{
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime EmployerApprovedOnDate { get; set; }
}
