namespace SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

public class GetShortCoursesForEarningsRequest : PagedQuery, IQuery
{
    public long UkPrn { get; }
    public int CollectionYear { get; }

    public GetShortCoursesForEarningsRequest(long ukPrn, int collectionYear, int page, int? pageSize)
    {
        UkPrn = ukPrn;
        CollectionYear = collectionYear;
        Page = page;
        PageSize = pageSize;
    }
}
