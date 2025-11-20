using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.DataTransferObjects;

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