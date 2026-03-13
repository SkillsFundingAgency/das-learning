using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

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

    public async Task Update(ShortCourseLearningDomainModel learning)
    {
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

    public async Task<ShortCourseLearningDomainModel> Get(string uln, bool unapprovedOnly=false)
    {
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == Guid.Empty) return null;

        var query = DbContext.Set<ShortCourseLearning>()
            .Where(x => x.LearnerKey == learnerKey);

        if (unapprovedOnly)
        {
            query = query
                .Include(x => x.Episodes.Where(e => e.IsApproved == false));
        }
        else
        {
            query = query
                .Include(x => x.Episodes);
        }

        var shortCourseLearning = await query.SingleOrDefaultAsync();

        if (shortCourseLearning is { Episodes.Count: 0 }) return null; //learnings without an episode are treated as if not exists

        return _learningFactory.GetExisting(shortCourseLearning);
    }

    public async Task<ShortCourseLearningDomainModel?> GetByLearnerKey(Guid learnerKey)
    {
        var shortCourseLearning = await DbContext
            .ShortCourseLearnings
            .IncludeAllChildren()
            .SingleOrDefaultAsync(x => x.LearnerKey == learnerKey);

        if (shortCourseLearning == null)
            return null;

        return _learningFactory.GetExisting(shortCourseLearning);
    }

    public async Task<PagedResult<Models.Dtos.Learning>> GetApprovedByDates(long ukPrn, DateRange dates, int limit, int offset, CancellationToken cancellationToken)
    {
        var baseQuery = DbContext.ShortCourseLearnings
            .Include(x => x.Episodes)
            .Where(x => x.Episodes.Any(e => e.Ukprn == ukPrn))
            .Where(x => x.Episodes.Any(e => e.IsApproved))
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

    public Task AddLearning(LearningDomainModel model)
    {
        if (model is not ShortCourseLearningDomainModel domainModel) throw new InvalidOperationException();
        return Add(domainModel);
    }

    public Task UpdateLearning(LearningDomainModel model)
    {
        if (model is not ShortCourseLearningDomainModel domainModel) throw new InvalidOperationException();
        return Update(domainModel);
    }

    async Task<LearningDomainModel?> ILearningRepository.GetUnapprovedLearning(string uln, long apprenticeshipId)
    {
        return await Get(uln, true);
    }
}

internal static class ShortCourseDbContextExtensions
{
    public static IQueryable<ShortCourseLearning> IncludeAllChildren(this DbSet<ShortCourseLearning> dbSet)
    {
        return dbSet
            .Include(x => x.Episodes)
            .ThenInclude(x => x.LearningSupport)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Milestones);
    }
}