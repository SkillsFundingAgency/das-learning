namespace SFA.DAS.Learning.Types;

#pragma warning disable CS8618

public class PaymentsStatusUpdatedForEpisode
{
    public string LearnerKey { get; set; }
    public string LearningKey { get; set; }
    public string EpisodeKey { get; set; }
    public bool PaymentsFrozen { get; set; } 
}

#pragma warning restore CS8618