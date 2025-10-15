namespace SFA.DAS.Learning.Types;

public class PersonalDetailsChangedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? EmailAddress { get; set; } = null!;
}