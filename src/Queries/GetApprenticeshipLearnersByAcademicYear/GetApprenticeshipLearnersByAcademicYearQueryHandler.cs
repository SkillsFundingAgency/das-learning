using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public class GetApprenticeshipLearnersByAcademicYearQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetApprenticeshipLearnersByAcademicYearRequest, GetApprenticeshipLearnersByAcademicYearResponse>
{
    public async Task<GetApprenticeshipLearnersByAcademicYearResponse> Handle(GetApprenticeshipLearnersByAcademicYearRequest query, CancellationToken cancellationToken = default)
    {
        var dates = AcademicYearParser.ParseFrom(query.AcademicYear);

        var baseQuery = dbContext.ApprenticeshipLearningDbSet
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn && !e.IsRemoved))
            .IsActiveInYear(dates.Start, dates.End)
            .AsNoTracking();

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(x => x.Episodes.Min(e => e.ApprovalsApprenticeshipId))
            .Skip(query.Offset)
            .Take(query.Limit)
            .Join(
                dbContext.LearnersDbSet.AsNoTracking(),
                learning => learning.LearnerKey,
                learner => learner.Key,
                (learning, learner) => new GetApprenticeshipLearnersByAcademicYearResponseItem
                {
                    Uln = learner.Uln,
                    Key = learner.Key
                })
            .ToListAsync(cancellationToken);

        return new GetApprenticeshipLearnersByAcademicYearResponse
        {
            Items = items,
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = totalItems
        };
    }
}
