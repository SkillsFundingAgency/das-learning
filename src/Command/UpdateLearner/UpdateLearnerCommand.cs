using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Command.UpdateLearner;

public class UpdateLearnerCommand : ICommand
{
    public Guid LearnerKey { get; }
    public LearnerUpdateModel UpdateModel { get; }
    public UpdateLearnerCommand(Guid learnerKey, LearnerUpdateModel updateModel)
    {
        LearnerKey = learnerKey;
        UpdateModel = updateModel;
    }
}


