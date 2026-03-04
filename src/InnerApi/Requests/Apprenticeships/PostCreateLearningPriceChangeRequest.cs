namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

public class PostCreateLearningPriceChangeRequest
{
    public string Initiator { get; set; }
    public string UserId { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? AssessmentPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Reason { get; set; }
    public DateTime EffectiveFromDate { get; set; }
}