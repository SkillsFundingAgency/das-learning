using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseDomainUpdateResult
{
    public Guid EpisodeKey { get; set; }
    public ShortCourseUpdateChanges[] Changes { get; set; } = [];
}
