using SFA.DAS.Learning.Command.Shared;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandResponse
{
    public List<CreateDraftShortCourseItemResult> Results { get; set; } = new();
}

public class CreateDraftShortCourseItemResult : ShortCourseLearningDto
{
    public bool IsReinstated { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsRemoved { get; set; }
}
