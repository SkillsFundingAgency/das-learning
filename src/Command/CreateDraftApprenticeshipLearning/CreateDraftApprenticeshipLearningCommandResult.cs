using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

#pragma warning disable CS8618 // Required properties must be set in the constructor
public class CreateDraftApprenticeshipLearningCommandResult
{
    public List<LearningUpdateChanges> Changes { get; set; }
    public Guid LearningKey { get; set; }
    public Guid LearningEpisodeKey { get; set; }
    public List<UpdateLearnerResult.EpisodePrice> Prices { get; set; }
    public Guid? RemovedLearningKey { get; set; }
}
#pragma warning restore CS8618 // Required properties must be set in the constructor