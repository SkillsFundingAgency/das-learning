using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand(long ukprn, int academicYear, List<ShortCourseUpdateContext> models): ICommand
{
    public long Ukprn { get; } = ukprn;
    public int AcademicYear { get; } = academicYear;
    public List<ShortCourseUpdateContext> Models { get; } = models;
}
