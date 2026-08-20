namespace SFA.DAS.Learning.Command.RemoveLearnerCommand;

public class RemoveLearnerCommand : ICommand
{
    public Guid LearnerKey { get; set; }
    public long Ukprn { get; set; }
    public int AcademicYear { get; set; }
    public RemoveLearnerCommand(Guid learnerKey, long ukprn, int academicYear)
    {
        LearnerKey = learnerKey;
        Ukprn = ukprn;
        AcademicYear = academicYear;
    }
}
