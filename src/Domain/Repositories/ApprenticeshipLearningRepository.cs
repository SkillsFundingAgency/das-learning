using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ApprenticeshipLearningRepository : IApprenticeshipLearningRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private IDomainEventDispatcher _domainEventDispatcher;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private LearningDataContext DbContext => _lazyContext.Value;

    public ApprenticeshipLearningRepository(Lazy<LearningDataContext> dbContext, IDomainEventDispatcher domainEventDispatcher, IApprenticeshipLearningFactory learningFactory)
    {
        _lazyContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _learningFactory = learningFactory;
    }

    public async Task Add(ApprenticeshipLearningDomainModel learning)
    {
        var entity = learning.GetEntity();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
            
        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task<ApprenticeshipLearningDomainModel> Get(Guid key)
    {
        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.MathsAndEnglishCourses).ThenInclude(y => y.BreaksInLearning)
            .Include(x => x.Episodes).ThenInclude(y => y.Prices)
            .Include(x => x.Episodes).ThenInclude(y => y.LearningSupport)
            .Include(x => x.Episodes).ThenInclude(y => y.BreaksInLearning)
            .SingleAsync(x => x.Key == key);

        return _learningFactory.GetExisting(apprenticeship);
    }

    public async Task<ApprenticeshipLearningDomainModel?> Get(
        string uln,
        long approvalsApprenticeshipId)
    {
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == default)
            return null;

        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Where(x => x.LearnerKey == learnerKey &&
                        x.ApprovalsApprenticeshipId == approvalsApprenticeshipId)
            .Include(x => x.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
                .ThenInclude(e => e.Prices)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return apprenticeship == null
            ? null
            : _learningFactory.GetExisting(apprenticeship);
    }

    public async Task<ApprenticeshipLearningDomainModel?> GetByUln(string uln)
    {
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == default)
            return null;

        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Where(x => x.LearnerKey == learnerKey)
            .Include(x => x.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
                .ThenInclude(e => e.Prices)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return apprenticeship == null
            ? null
            : _learningFactory.GetExisting(apprenticeship);
    }

    public async Task Update(ApprenticeshipLearningDomainModel learning)
    {
        await DbContext.SaveChangesAsync();
  
        foreach (dynamic domainEvent in learning.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public Task AddLearning(LearningDomainModel model)
    {
        if (model is not ApprenticeshipLearningDomainModel domainModel) throw new InvalidOperationException();
        return Add(domainModel);
    }

    public Task UpdateLearning(LearningDomainModel model)
    {
        if (model is not ApprenticeshipLearningDomainModel domainModel) throw new InvalidOperationException();
        return Update(domainModel);
    }

    async Task<LearningDomainModel?> ILearningRepository.GetUnapprovedLearning(string uln, long apprenticeshipId)
    {
        //Nb, currently all ApprenticeshipLearning is regarded as being "unapproved"
        return await Get(uln, apprenticeshipId);
    }
}