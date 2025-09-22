namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.EpisodePrice")]
[System.ComponentModel.DataAnnotations.Schema.Table("EpisodePrice")]
public class EpisodePrice
{
    [Key]
    public Guid Key { get; set; }
	public Guid EpisodeKey { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? EndPointAssessmentPrice { get; set; }
    public decimal TotalPrice { get; set; }
	public int FundingBandMaximum { get; set; }
}