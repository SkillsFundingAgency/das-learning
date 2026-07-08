using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommand : ICommand
{
    public Guid LearnerKey { get; }
    public LearningUpdateContext UpdateModel { get; }
    public UpdateLearnerCommand(Guid learnerKey, LearningUpdateContext updateModel)
    {
        LearnerKey = learnerKey;
        UpdateModel = updateModel;
    }
}