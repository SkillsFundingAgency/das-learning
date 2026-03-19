using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(ShortCourseUpdateContext model)
    {
        Model = model;
    }

    public ShortCourseUpdateContext Model { get; }
}