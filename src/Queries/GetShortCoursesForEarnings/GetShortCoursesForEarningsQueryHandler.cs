using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

public class GetShortCoursesForEarningsQueryHandler(LearningDataContext dbContext)
    : IQueryHandler<GetShortCoursesForEarningsRequest, GetShortCoursesForEarningsResponse>
{
    public async Task<GetShortCoursesForEarningsResponse> Handle(GetShortCoursesForEarningsRequest query, CancellationToken cancellationToken = default)
    {
        var dates = AcademicYearParser.ParseFrom(query.CollectionYear);

        var baseQuery = dbContext.ShortCourseLearnings
            .Include(x => x.Episodes)
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn))
            .Where(x => x.Episodes.Any(e =>
                e.StartDate <= dates.End &&
                e.ExpectedEndDate >= dates.Start &&
                (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= dates.Start)))
            .AsNoTracking();

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var learnings = await baseQuery
            .OrderBy(x => x.Key)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        var learnerKeys = learnings.Select(x => x.LearnerKey).ToList();
        var learners = await dbContext.LearnersDbSet
            .Where(l => learnerKeys.Contains(l.Key))
            .AsNoTracking()
            .ToDictionaryAsync(l => l.Key, cancellationToken);

        return new GetShortCoursesForEarningsResponse
        {
            Items = learnings.Select(l =>
            {
                learners.TryGetValue(l.LearnerKey, out var learner);
                return new GetShortCoursesForEarningsItem
                {
                    LearningKey = l.Key,
                    Learner = new GetShortCoursesForEarningsLearner
                    {
                        Uln = learner?.Uln,
                        FirstName = learner?.FirstName,
                        LastName = learner?.LastName,
                        DateOfBirth = learner?.DateOfBirth ?? default
                    },
                    Episodes = l.Episodes.Select(e => new GetShortCoursesForEarningsEpisode
                    {
                        CourseCode = e.TrainingCode,
                        IsApproved = e.IsApproved,
                        Price = e.Price
                    })
                };
            }),
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = totalItems
        };
    }
}
