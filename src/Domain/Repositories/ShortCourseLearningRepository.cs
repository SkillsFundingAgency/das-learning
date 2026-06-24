using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ShortCourseLearningRepository : IShortCourseLearningRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private readonly IShortCourseLearningFactory _learningFactory;

    private LearningDataContext DbContext => _lazyContext.Value;

    private readonly IUnitOfWork _unitOfWork;

    public ShortCourseLearningRepository(
        Lazy<LearningDataContext> dbContext,
        IShortCourseLearningFactory learningFactory,
        IUnitOfWork unitOfWork)
    {
        _lazyContext = dbContext;
        _learningFactory = learningFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task Add(ShortCourseLearningDomainModel learning)
    {
        var entity = learning.GetEntity();
        await DbContext.AddAsync(entity);
        _unitOfWork.Track(learning);
    }

    public Task Update(ShortCourseLearningDomainModel learning)
    {
        _unitOfWork.Track(learning);
        return Task.CompletedTask;
    }

    public async Task<ShortCourseLearningDomainModel> Get(Guid key)
    {
        var shortCourseLearning = await DbContext.ShortCourseLearnings
            .IncludeAllChildren()
            .SingleAsync(x => x.Key == key);

        return _learningFactory.GetExisting(shortCourseLearning);
    }

    public async Task<ShortCourseLearningDomainModel> Get(string uln, bool unapprovedOnly = false, string? trainingCode = null)
    {
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == Guid.Empty) return null;

        var query = DbContext.Set<ShortCourseLearning>()
            .Where(x => x.LearnerKey == learnerKey);

        if (trainingCode != null)
            query = query.Where(x => x.TrainingCode == trainingCode);

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

    public async Task<List<ShortCourseLearningDomainModel>> GetAllByLearnerKey(Guid learnerKey)
    {
        var learnings = await DbContext
            .ShortCourseLearnings
            .IncludeAllChildren()
            .Where(x => x.LearnerKey == learnerKey)
            .ToListAsync();

        return learnings.Select(_learningFactory.GetExisting).ToList();
    }

    public async Task<ShortCourseLearningDomainModel?> GetByLearnerKeyAndCourseCode(Guid learnerKey, string courseCode)
    {
        var shortCourseLearning = await DbContext
            .ShortCourseLearnings
            .IncludeAllChildren()
            .SingleOrDefaultAsync(x => x.LearnerKey == learnerKey && x.TrainingCode == courseCode);

        if (shortCourseLearning == null)
            return null;

        if (shortCourseLearning.Episodes.Count == 0)
            return null;

        return _learningFactory.GetExisting(shortCourseLearning);
    }

    public async Task<PagedResult<Models.Dtos.Learning>> GetApprovedByDates(long ukPrn, DateRange dates, int limit, int offset, CancellationToken cancellationToken)
    {
        var baseQuery = DbContext.ShortCourseLearnings
            .Include(x => x.Episodes)
            .Where(x => x.Episodes.Any(e => 
                e.Ukprn == ukPrn &&
                e.IsApproved &&
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

    async Task<LearningDomainModel?> ILearningRepository.GetUnapprovedLearning(string uln, long apprenticeshipId, string? trainingCode = null)
    {
        return await Get(uln, unapprovedOnly: true, trainingCode: trainingCode);
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