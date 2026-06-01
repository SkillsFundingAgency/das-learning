using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommand : ICommand
{
    public CreateDraftApprenticeshipLearningCommand(long ukprn, LearningUpdateContext learningUpdateContext)
    {
        Ukprn = ukprn;
        LearningUpdateContext = learningUpdateContext;
    }

    public long Ukprn { get; }
    public LearningUpdateContext LearningUpdateContext { get; }
}
