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
        var shortCourseLearning = await DbContext.ShortCourseLearnings
            .IncludeAllChildren()
            .SingleAsync(x => x.Key == key);

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