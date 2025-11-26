namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Calendar;

public class GetAcademicYearsResponse
{
#pragma warning disable CS8618
    public string AcademicYear { get; set; }
#pragma warning restore CS8618
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? HardCloseDate { get; set; }
}