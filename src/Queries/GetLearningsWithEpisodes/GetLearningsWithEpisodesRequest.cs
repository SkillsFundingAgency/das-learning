namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesRequest : IQuery
{
    public long Ukprn { get; set; }
    public List<Guid> LearningKeys { get; set; } = [];
}
