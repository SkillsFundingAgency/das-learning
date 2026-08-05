using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandResult : ShortCourseLearningDto
{
    public bool IsReinstated { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsRemoved { get; set; }
}
