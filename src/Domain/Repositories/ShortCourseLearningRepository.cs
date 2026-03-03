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

    public async Task<ShortCourseLearningDomainModel> Get(string uln)
    {
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == default)
            return null;

        var shortCourseLearning = await DbContext.Set<ShortCourseLearning>()
            .Include(x => x.Episodes)
            .SingleAsync(x => x.Key == learnerKey);

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

    async Task<LearningDomainModel?> ILearningRepository.GetLearning(string uln, long apprenticeshipId)
    {
        return await Get(uln);
    }
}