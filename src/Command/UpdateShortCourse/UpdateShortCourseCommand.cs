using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommand : ICommand
{
    public Guid LearningKey { get; }
    public ShortCourseUpdateContext Model { get; }

    public UpdateShortCourseCommand(Guid learningKey, ShortCourseUpdateContext model)
    {
        LearningKey = learningKey;
        Model = model;
    }
}

public class UpdateShortCourseResult
{
    public Guid LearningKey { get; set; }
    public ShortCourseUpdateChanges[] Changes { get; set; } = [];
}
