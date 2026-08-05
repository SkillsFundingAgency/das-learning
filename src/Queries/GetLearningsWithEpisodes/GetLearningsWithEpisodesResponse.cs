using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesResponse : PagedQueryResult<LearningWithEpisodes> {}

[ExcludeFromCodeCoverage]
public class LearningWithEpisodes
{
    public LearningWithEpisodes(
        Guid key,
        string uln,
        DateTime startDate,
        DateTime plannedEndDate,
        List<Episode> episodes,
        int ageAtStartOfLearning,
        DateTime? lastDayOfLearning,
        DateTime? completionDate)
    {
        Key = key;
        Uln = uln;
        StartDate = startDate;
        PlannedEndDate = plannedEndDate;
        Episodes = episodes;
        AgeAtStartOfLearning = ageAtStartOfLearning;
        LastDayOfLearning = lastDayOfLearning;
        CompletionDate = completionDate;
    }

    public Guid Key { get; set; }
    public string Uln { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public List<Episode> Episodes { get; set; }
    public int AgeAtStartOfApprenticeship => AgeAtStartOfLearning;
    public int AgeAtStartOfLearning { get; set; }
    [JsonPropertyName("WithdrawnDate")]// Because of multiple inflight tickets, we will need to manage the switch over
    public DateTime? LastDayOfLearning { get; set; }
    public DateTime? CompletionDate { get; set; }

    [ExcludeFromCodeCoverage]
    public class Episode
    {
        public Episode(Guid key, string trainingCode, DateTime? lastDayOfLearning, List<EpisodePrice> prices)
        {
            Key = key;
            TrainingCode = trainingCode;
            LastDayOfLearning = lastDayOfLearning;
            Prices = prices;
        }

        public Guid Key { get; set; }
        public string TrainingCode { get; set; }
        public DateTime? LastDayOfLearning { get; set; }
        public List<EpisodePrice> Prices { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EpisodePrice
    {
        public EpisodePrice(Guid key, DateTime startDate, DateTime endDate, decimal? trainingPrice, decimal? endPointAssessmentPrice, decimal totalPrice)
        {
            Key = key;
            StartDate = startDate;
            EndDate = endDate;
            TrainingPrice = trainingPrice;
            EndPointAssessmentPrice = endPointAssessmentPrice;
            TotalPrice = totalPrice;
        }

        public Guid Key { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? TrainingPrice { get; set; }
        public decimal? EndPointAssessmentPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
