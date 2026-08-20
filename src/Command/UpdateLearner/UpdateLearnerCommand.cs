using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommand : ICommand
{
    public Guid LearnerKey { get; }
    public long Ukprn { get; }
    public string TrainingCode { get; }
    public LearningUpdateContext UpdateModel { get; }
    public UpdateLearnerCommand(Guid learnerKey, long ukprn, string trainingCode, LearningUpdateContext updateModel)
    {
        LearnerKey = learnerKey;
        Ukprn = ukprn;
        TrainingCode = trainingCode;
        UpdateModel = updateModel;
    }
}