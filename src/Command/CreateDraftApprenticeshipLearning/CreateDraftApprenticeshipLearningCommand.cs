using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

public class CreateDraftApprenticeshipLearningCommand : ICommand
{
    public CreateDraftApprenticeshipLearningCommand(long ukprn, LearningUpdateContext learningUpdateContext,
        string trainingCode, int academicYear)
    {
        Ukprn = ukprn;
        LearningUpdateContext = learningUpdateContext;
        TrainingCode = trainingCode;
        AcademicYear = academicYear;
    }

    public long Ukprn { get; }
    public LearningUpdateContext LearningUpdateContext { get; }
    public string TrainingCode { get; }
    public int AcademicYear { get; }
}
