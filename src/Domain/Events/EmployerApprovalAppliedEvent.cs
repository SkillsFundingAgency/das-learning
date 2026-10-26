using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Events;

public class EmployerApprovalAppliedEvent : IDomainEvent
{
    public Guid LearnerKey { get; set; }
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
    public long ApprenticeshipId { get; set; }
    public LearningType LearningType { get; set; }
    public List<ApprovedChange> Changes { get; set; } = [];
}
