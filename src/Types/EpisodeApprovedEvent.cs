namespace SFA.DAS.Learning.Types;

public class EpisodeApprovedEvent
{
    public Guid LearnerKey { get; set; }
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
}
