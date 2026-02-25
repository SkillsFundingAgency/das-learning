namespace SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

public class GetShortCoursesByAcademicYearRequest : PagedQuery, IQuery
{
    public long UkPrn { get; }
    public int AcademicYear { get; }

    public GetShortCoursesByAcademicYearRequest(long ukPrn, int academicYear, int page, int? pageSize)
    {
        UkPrn = ukPrn;
        AcademicYear = academicYear;
        Page = page;
        PageSize = pageSize;
    }
}
