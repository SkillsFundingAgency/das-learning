using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommandResult : ShortCourseLearningDto
{
    public string CourseCode { get; set; } = "";
    public Guid EpisodeKey { get; set; }
    public bool IsReinstated { get; set; }
    public bool IsIgnored { get; set; }
    public bool IsRemoved { get; set; }
}
