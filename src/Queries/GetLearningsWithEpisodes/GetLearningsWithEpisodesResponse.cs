using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesResponse : PagedQueryResult<LearningWithEpisodes> {}

[ExcludeFromCodeCoverage]
public class LearningWithEpisodes(
    Guid key,
    string uln,
    DateTime startDate,
    DateTime plannedEndDate,
    List<LearningWithEpisodes.Episode> episodes,
    int ageAtStartOfLearning,
    DateTime? lastDayOfLearning,
    DateTime? completionDate)
{
    public Guid Key { get; set; } = key;
    public string Uln { get; set; } = uln;
    public DateTime StartDate { get; set; } = startDate;
    public DateTime PlannedEndDate { get; set; } = plannedEndDate;
    public List<Episode> Episodes { get; set; } = episodes;
    public int AgeAtStartOfApprenticeship => AgeAtStartOfLearning;
    public int AgeAtStartOfLearning { get; set; } = ageAtStartOfLearning;

    [JsonPropertyName("WithdrawnDate")]// Because of multiple inflight tickets, we will need to manage the switch over
    public DateTime? LastDayOfLearning { get; set; } = lastDayOfLearning;

    public DateTime? CompletionDate { get; set; } = completionDate;

    [ExcludeFromCodeCoverage]
    public class Episode(Guid key, string trainingCode, DateTime? lastDayOfLearning, List<EpisodePrice> prices)
    {
        public Guid Key { get; set; } = key;
        public string TrainingCode { get; set; } = trainingCode;
        public DateTime? LastDayOfLearning { get; set; } = lastDayOfLearning;
        public List<EpisodePrice> Prices { get; set; } = prices;
    }

    [ExcludeFromCodeCoverage]
    public class EpisodePrice(
        Guid key,
        DateTime startDate,
        DateTime endDate,
        decimal? trainingPrice,
        decimal? endPointAssessmentPrice,
        decimal totalPrice)
    {
        public Guid Key { get; set; } = key;
        public DateTime StartDate { get; set; } = startDate;
        public DateTime EndDate { get; set; } = endDate;
        public decimal? TrainingPrice { get; set; } = trainingPrice;
        public decimal? EndPointAssessmentPrice { get; set; } = endPointAssessmentPrice;
        public decimal TotalPrice { get; set; } = totalPrice;
    }
}
