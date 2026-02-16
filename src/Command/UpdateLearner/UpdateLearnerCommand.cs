using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommand : ICommand
{
    public Guid LearningKey { get; }
    public LearningUpdateContext UpdateModel { get; }
    public UpdateLearnerCommand(Guid learningKey, LearningUpdateContext updateModel)
    {
        LearningKey = learningKey;
        UpdateModel = updateModel;
    }
}