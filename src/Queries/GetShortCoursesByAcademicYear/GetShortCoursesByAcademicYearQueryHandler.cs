using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

public class GetShortCoursesByAcademicYearQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>
{
    public async Task<GetShortCoursesByAcademicYearResponse> Handle(GetShortCoursesByAcademicYearRequest query, CancellationToken cancellationToken = default)
    {
        var dates = AcademicYearParser.ParseFrom(query.AcademicYear);

        var baseQuery = dbContext.ShortCourseLearnings
            .Include(x => x.Episodes)
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn))
            .Where(x => x.Episodes.Any(e => e.IsApproved))
            .Where(x => x.Episodes.Any(e =>
                e.StartDate <= dates.End &&
                (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= dates.Start)))
            .Where(x => !x.CompletionDate.HasValue || x.CompletionDate.Value >= dates.Start)
            .AsNoTracking();

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(x => x.Key)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Join(
                dbContext.LearnersDbSet.AsNoTracking(),
                learning => learning.LearnerKey,
                learner => learner.Key,
                (learning, learner) => new ShortCourseLearnerItem
                {
                    Uln = learner.Uln,
                    Key = learning.Key
                })
            .ToListAsync(cancellationToken);

        return new GetShortCoursesByAcademicYearResponse
        {
            Items = items,
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = totalItems
        };
    }
}
