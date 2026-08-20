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

        var matchingLearnerKeys = dbContext.ApprenticeshipLearningDbSet
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn && !e.IsRemoved && e.IsApproved))
            .IsActiveInYear(dates.Start, dates.End)
            .AsNoTracking()
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
                (key, learner) => new GetLearningsByDatesResponseItem
                {
                    Uln = learner.Uln,
                    Key = learner.Key
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
