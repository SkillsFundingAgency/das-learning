using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommand : ICommand
{
    public CreateDraftApprenticeshipLearningCommand(long ukprn, LearningUpdateContext learningUpdateContext,
        string trainingCode)
    {
        Ukprn = ukprn;
        LearningUpdateContext = learningUpdateContext;
        TrainingCode = trainingCode;
    }

    public long Ukprn { get; }
    public LearningUpdateContext LearningUpdateContext { get; }
    public string TrainingCode { get; }
}
