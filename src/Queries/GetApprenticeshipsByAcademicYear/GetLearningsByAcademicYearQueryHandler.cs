using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
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
            .Where(x => x.Episodes.Any(e =>
                e.Prices.Any(p =>
                    p.StartDate <= dates.End &&
                    p.EndDate >= dates.Start &&
                    (!e.WithdrawalDate.HasValue ||
                        (e.WithdrawalDate.Value >= dates.Start &&
                         e.WithdrawalDate != p.StartDate)))))
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
