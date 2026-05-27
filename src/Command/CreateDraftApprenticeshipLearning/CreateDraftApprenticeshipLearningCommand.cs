using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommand : ICommand
{
    public CreateDraftApprenticeshipLearningCommand(long ukprn, string uln, LearningUpdateContext learningUpdateContext)
    {
        Ukprn = ukprn;
        Uln = uln;
        LearningUpdateContext = learningUpdateContext;
    }

    public long Ukprn { get; }
    public string Uln { get; }
    public LearningUpdateContext LearningUpdateContext { get; }
}
