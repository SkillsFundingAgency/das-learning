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
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.UkPrn && !e.IsRemoved))
            .Where(x => x.Episodes.Any(e => 
                e.Ukprn == query.UkPrn &&
                !e.IsRemoved &&
                e.StartDate <= dates.End &&
                (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= dates.Start) &&
                (!e.CompletionDate.HasValue || e.CompletionDate.Value >= dates.Start)))
            .AsNoTracking();

        var learnerKeysQuery = baseQuery.Select(x => x.LearnerKey).Distinct();

        var totalItems = await learnerKeysQuery.CountAsync(cancellationToken);

        var pagedLearnerKeys = await learnerKeysQuery
            .OrderBy(k => k)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        var learnings = await baseQuery
            .Where(x => pagedLearnerKeys.Contains(x.LearnerKey))
            .ToListAsync(cancellationToken);

        var learners = await dbContext.LearnersDbSet
            .Where(l => pagedLearnerKeys.Contains(l.Key))
            .AsNoTracking()
            .ToDictionaryAsync(l => l.Key, cancellationToken);

        return new GetShortCoursesForEarningsResponse
        {
            Items = pagedLearnerKeys.Select(learnerKey =>
            {
                learners.TryGetValue(learnerKey, out var learner);
                var learnerLearnings = learnings.Where(l => l.LearnerKey == learnerKey);
                return new GetShortCoursesForEarningsItem
                {
                    LearnerKey = learnerKey,
                    Learner = new GetShortCoursesForEarningsLearner
                    {
                        Uln = learner?.Uln,
                        FirstName = learner?.FirstName,
                        LastName = learner?.LastName,
                        DateOfBirth = learner?.DateOfBirth ?? default
                    },
                    Episodes = learnerLearnings.SelectMany(l => l.Episodes
                        .Where(e => !e.IsRemoved && e.Ukprn == query.UkPrn)
                        .Select(e => new GetShortCoursesForEarningsEpisode
                        {
                            LearningKey = l.Key,
                            CourseCode = l.TrainingCode,
                            IsApproved = e.IsApproved,
                            Price = e.Price,
                            LearnerRef = e.LearnerRef,
                            EmployerType = e.EmployerType
                        }))
                };
            }),
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = totalItems
        };
    }
}
