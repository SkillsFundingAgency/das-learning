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

    public async Task Update(ShortCourseLearningDomainModel learning)
    {
        await DbContext.SaveChangesAsync();

        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
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