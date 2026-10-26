using SFA.DAS.Learning.Command.UpdateLearner;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

/// <summary>
/// Inherits from the PUT result because a POST that reinstates a removed learning goes through the same
/// update as a PUT, so it can make changes that need further approval.
/// </summary>
public class CreateDraftApprenticeshipLearningCommandResult : UpdateLearnerResult
{
    public Guid? RemovedLearningKey { get; set; }
}
