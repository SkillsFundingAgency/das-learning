namespace SFA.DAS.Learning.Command.FreezeLearning;

// Note: Learning can be Frozen by either ApprenticeshipPausedEvent or ApprenticeshipStoppedEvent. Externally these
// mean different things, but internally they both mean the same thing: Learning is frozen.
// Therefore, we can use the same command to handle both events.
public class FreezeLearningCommand : ICommand
{
	public FreezeLearningCommand(long approvalsApprenticeshipId)
	{
		ApprovalsApprenticeshipId = approvalsApprenticeshipId;
	}

	public long ApprovalsApprenticeshipId { get; }
}
