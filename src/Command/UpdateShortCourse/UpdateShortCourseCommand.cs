using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommand : ICommand
{
    public Guid LearnerKey { get; }
    public long Ukprn { get; }
    public int AcademicYear { get; }
    public List<ShortCourseUpdateContext> Models { get; }

    public UpdateShortCourseCommand(Guid learnerKey, long ukprn, int academicYear, List<ShortCourseUpdateContext> models)
    {
        LearnerKey = learnerKey;
        Ukprn = ukprn;
        AcademicYear = academicYear;
        Models = models;
    }
}