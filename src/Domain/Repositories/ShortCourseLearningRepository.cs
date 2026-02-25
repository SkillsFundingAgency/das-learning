using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ShortCourseLearningRepository : IShortCourseLearningRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IShortCourseLearningFactory _learningFactory;

    private LearningDataContext DbContext => _lazyContext.Value;

    public ShortCourseLearningRepository(
        Lazy<LearningDataContext> dbContext,
        IDomainEventDispatcher domainEventDispatcher,
        IShortCourseLearningFactory learningFactory)
    {
        _lazyContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _learningFactory = learningFactory;
    }

    public async Task Add(ShortCourseLearningDomainModel learning)
    {
        var entity = learning.GetEntity();

        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();

        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task<ShortCourseLearningDomainModel> Get(Guid key)
    {
        var shortCourseLearning = await DbContext.Set<ShortCourseLearning>()
            .Include(x => x.Episodes)
            .SingleAsync(x => x.Key == key);

        return _learningFactory.GetExisting(shortCourseLearning);
    }

    public async Task<PagedResult<Models.Dtos.Learning>> GetByDates(long ukPrn, DateRange dates, int limit, int offset, CancellationToken cancellationToken)
    {
        var baseQuery = DbContext.ShortCourseLearnings
            .Where(x => x.Episodes.Any(e => e.Ukprn == ukPrn))
            .Where(x => x.Episodes.Any(e =>
                e.StartDate <= dates.End &&
                e.ExpectedEndDate >= dates.Start &&
                (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= dates.Start)))
            .AsNoTracking();

        var totalItems = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);

        var result = await baseQuery
            .OrderBy(x => x.Key)
            .Skip(offset)
            .Take(limit)
            .Join(
                DbContext.LearnersDbSet.AsNoTracking(),
                learning => learning.LearnerKey,
                learner => learner.Key,
                (learning, learner) => new Models.Dtos.Learning
                {
                    Uln = learner.Uln,
                    Key = learning.Key
                })
            .ToListAsync(cancellationToken);

        return new PagedResult<Models.Dtos.Learning>
        {
            Data = result,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
