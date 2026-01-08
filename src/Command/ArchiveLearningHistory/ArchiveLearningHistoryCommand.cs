using SFA.DAS.Learning.Domain.Events;

namespace SFA.DAS.Learning.Command.ArchiveLearningHistory;

public class ArchiveLearningHistoryCommand : ICommand
{
    public LearnerUpdatedEvent LearnerUpdatedEvent { get; set; }
    public ArchiveLearningHistoryCommand(LearnerUpdatedEvent learnerUpdatedEvent)
    {
        LearnerUpdatedEvent = learnerUpdatedEvent;
    }
}