using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Extensions;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public class GetLearningsByAcademicYearQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetLearningsByAcademicYearRequest, GetLearningsByAcademicYearResponse>
{
    public async Task<GetLearningsByAcademicYearResponse> Handle(GetLearningsByAcademicYearRequest query, CancellationToken cancellationToken = default)
    {
        var dates = AcademicYearParser.ParseFrom(query.AcademicYear);

        var baseQuery = dbContext.ApprenticeshipLearningDbSet
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn))
            .IsActiveInYear(dates.Start, dates.End)
            .AsNoTracking();

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(x => x.ApprovalsApprenticeshipId)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Join(
                dbContext.LearnersDbSet.AsNoTracking(),
                learning => learning.LearnerKey,
                learner => learner.Key,
                (learning, learner) => new GetLearningsByDatesResponseItem
                {
                    Uln = learner.Uln,
                    Key = learning.Key
                })
            .ToListAsync(cancellationToken);

        return new GetLearningsByAcademicYearResponse
        {
            Items = items,
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = totalItems
        };
    }
}
