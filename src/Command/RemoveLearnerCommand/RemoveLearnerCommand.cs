namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommand : ICommand
{
    public Guid LearnerKey { get; set; }
    public RemoveLearnerCommand(Guid learnerKey)
    {
        LearnerKey = learnerKey;
    }
}
