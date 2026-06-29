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

        var matchingLearnerKeys = dbContext.ShortCourseLearnings
            .Where(x => x.Episodes.Any(e =>
                e.Ukprn == query.UkPrn &&
                e.IsApproved && !e.IsRemoved &&
                e.StartDate <= dates.End &&
                (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= dates.Start) &&
                (!e.CompletionDate.HasValue || e.CompletionDate.Value >= dates.Start)))
            .Select(x => x.LearnerKey)
            .Distinct();

        var totalItems = await matchingLearnerKeys.CountAsync(cancellationToken);

        var items = await matchingLearnerKeys
            .OrderBy(k => k)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Join(
                dbContext.LearnersDbSet.AsNoTracking(),
                key => key,
                learner => learner.Key,
                (key, learner) => new ShortCourseLearnerItem
                {
                    Uln = learner.Uln,
                    Key = learner.Key
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
