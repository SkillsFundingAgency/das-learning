namespace SFA.DAS.Learning.Types;

public class ApprovedLearningUpdatedEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public string LearningType { get; set; } = null!;
    public List<ApprenticeshipFieldChange> Changes { get; set; } = new();
}

public class ApprenticeshipFieldChange
{
    public string ChangeType { get; set; } = null!;
    public ApprenticeshipData Data { get; set; } = null!;
}

public class ApprenticeshipData
{
    public string Old { get; set; } = string.Empty;
    public string New { get; set; } = string.Empty;
}
