using System.Text.Json.Serialization;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.Dtos;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommand : ICommand
{
    public Guid LearnerKey { get; }
    public List<ShortCourseUpdateContext> Models { get; }

    public UpdateShortCourseCommand(Guid learnerKey, List<ShortCourseUpdateContext> models)
    {
        LearnerKey = learnerKey;
        Models = models;
    }
}

public class UpdateShortCourseResult : ShortCourseLearningDto
{
    public string CourseCode { get; set; } = "";
    public ShortCourseUpdateChanges[] Changes { get; set; } = [];
    public Guid UpdatedEpisodeKey { get; set; }
    public bool IsNewLearning { get; set; }
    public bool IsIgnored { get; set; }
}
