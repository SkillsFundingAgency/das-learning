using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.ClearFurtherApprovalNeeded;

public class ClearFurtherApprovalNeededCommand : ICommand
{
    public LearningType LearningType { get; set; }
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
}
