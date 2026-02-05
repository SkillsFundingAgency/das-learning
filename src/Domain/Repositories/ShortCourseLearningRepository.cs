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
}