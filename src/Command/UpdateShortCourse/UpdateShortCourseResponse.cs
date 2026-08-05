using SFA.DAS.Learning.Command.Shared;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseResponse
{
    public List<UpdateShortCourseItemResult> Results { get; set; } = [];
}

public class UpdateShortCourseItemResult : ShortCourseLearningDto
{
    public ShortCourseUpdateChanges[] Changes { get; set; } = [];
    public bool IsNewLearning { get; set; }
    public bool IsNewEpisode { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsRemoved { get; set; }
}
