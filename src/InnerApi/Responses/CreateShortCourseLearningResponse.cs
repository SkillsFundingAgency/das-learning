namespace SFA.DAS.Learning.InnerApi.Responses;

#pragma warning disable CS8618
/// <summary>
/// Response when a short course learning is created, containing the keys for the learning and the episode.
/// </summary>
public class CreateShortCourseLearningResponse
{
    /// <summary>
    /// LearningKey for new or existing learning (that may have been updated)
    /// </summary>
    public Guid LearningKey { get; set; }
    /// <summary>
    /// EpisodeKey, although a learning may have multiple episodes, 
    /// only one episode will be created/updated when creating a draft short course, 
    /// so we can return the key for that single episode here. 
    /// </summary>
    public Guid EpisodeKey { get; set; }
}
#pragma warning restore CS8618