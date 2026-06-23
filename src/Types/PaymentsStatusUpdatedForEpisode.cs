namespace SFA.DAS.Learning.Types;

#pragma warning disable CS8618

public class PaymentsStatusUpdatedForEpisode
{
    public Guid LearnerKey { get; set; }
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public bool PaymentsFrozen { get; set; } 
}

#pragma warning restore CS8618