using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;

#pragma warning disable CS8618 // Required properties must be set in the constructor
public class CreateDraftApprenticeshipLearningCommandResult
{
    public List<LearningUpdateChanges> Changes { get; internal set; }
    public Guid LearningKey { get; internal set; }
    public Guid LearningEpisodeKey { get; internal set; }
    public List<UpdateLearnerResult.EpisodePrice> Prices { get; internal set; }
}
#pragma warning restore CS8618 // Required properties must be set in the constructor