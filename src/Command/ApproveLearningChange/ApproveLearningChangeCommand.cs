using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Command.ApproveLearningChange;

public class ApproveLearningChangeCommand : ICommand
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public List<ApprovedChange> Changes { get; set; } = new();
}
