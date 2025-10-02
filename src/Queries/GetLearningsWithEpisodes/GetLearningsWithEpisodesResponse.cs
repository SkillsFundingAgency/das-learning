using SFA.DAS.Learning.DataTransferObjects;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesResponse(long ukprn, List<LearningWithEpisodes> learnings)
{
    public long Ukprn { get; set; } = ukprn;
    public List<LearningWithEpisodes> Learnings { get; set; } = learnings;
}

