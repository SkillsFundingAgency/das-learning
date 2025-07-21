using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.DataTransferObjects;

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