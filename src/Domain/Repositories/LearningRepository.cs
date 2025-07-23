using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILearningFactory _learningFactory;
    private readonly IAccountIdAuthorizer _accountIdAuthorizer;
    private LearningDataContext DbContext => _lazyContext.Value;

    public LearningRepository(Lazy<LearningDataContext> dbContext, IDomainEventDispatcher domainEventDispatcher, ILearningFactory learningFactory, IAccountIdAuthorizer accountIdAuthorizer)
    {
        _lazyContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _learningFactory = learningFactory;
        _accountIdAuthorizer = accountIdAuthorizer;
    }

    public async Task Add(LearningDomainModel learning)
    {
        var entity = learning.GetEntity();
        _accountIdAuthorizer.AuthorizeAccountId(entity);
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
            
        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task<LearningDomainModel> Get(Guid key)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.PriceHistories)
            .Include(x => x.StartDateChanges)
            .Include(x => x.FreezeRequests)
            .Include(x => x.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .SingleAsync(x => x.Key == key);

        return _learningFactory.GetExisting(apprenticeship);
    }

    public async Task<LearningDomainModel?> Get(string uln, long approvalsApprenticeshipId)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.PriceHistories)
            .Include(x => x.StartDateChanges)
            .Include(x => x.FreezeRequests)
            .Include(x => x.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .SingleOrDefaultAsync(x => x.Uln == uln && x.ApprovalsApprenticeshipId == approvalsApprenticeshipId);
        return apprenticeship == null ? null : _learningFactory.GetExisting(apprenticeship);
    }
    
    public async Task<LearningDomainModel?> GetByUln(string uln)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .Include(x => x.PriceHistories)
            .Include(x => x.StartDateChanges)
            .Include(x => x.FreezeRequests)
            .Include(x => x.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .SingleOrDefaultAsync(x => x.Uln == uln);

        if (apprenticeship == null)
        {
            return null;
        }

        return _learningFactory.GetExisting(apprenticeship);
    }

    public async Task Update(LearningDomainModel learning)
    {
        var entity = learning.GetEntity();
        _accountIdAuthorizer.AuthorizeAccountId(entity);
        DbContext.Update(entity);

        await DbContext.SaveChangesAsync();
  
        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }
}