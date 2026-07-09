namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public class GetApprenticeshipLearnersByAcademicYearRequest : PagedQuery, IQuery
{
    public long UkPrn { get; }
    public int AcademicYear { get; }
    
    public GetApprenticeshipLearnersByAcademicYearRequest(long ukPrn, int academicYear, int page, int? pageSize)
    {
        UkPrn = ukPrn;
        AcademicYear = academicYear;
        Page = page;
        PageSize = pageSize;
    }
}