namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemovedLearning
{
    public Guid LearningKey { get; set; }
    public string? LearnerRef { get; set; }
}
