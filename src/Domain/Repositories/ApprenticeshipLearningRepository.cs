using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.Repositories;

public class ApprenticeshipLearningRepository : IApprenticeshipLearningRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private LearningDataContext DbContext => _lazyContext.Value;

    private readonly IUnitOfWork _unitOfWork;

    public ApprenticeshipLearningRepository(Lazy<LearningDataContext> dbContext, IApprenticeshipLearningFactory learningFactory, IUnitOfWork unitOfWork)
    {
        _lazyContext = dbContext;
        _learningFactory = learningFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task Add(ApprenticeshipLearningDomainModel learning)
    {
        var entity = learning.GetEntity();
        await DbContext.AddAsync(entity);
        _unitOfWork.Track(learning);
    }

    public async Task<ApprenticeshipLearningDomainModel> Get(Guid key)
    {
        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.EnglishAndMathsCourses).ThenInclude(y => y.BreaksInLearning)
            .Include(x => x.Episodes).ThenInclude(y => y.Prices)
            .Include(x => x.Episodes).ThenInclude(y => y.LearningSupport)
            .Include(x => x.Episodes).ThenInclude(y => y.BreaksInLearning)
            .SingleAsync(x => x.Key == key);

        return _learningFactory.GetExisting(apprenticeship);
    }

    public async Task<ApprenticeshipLearningDomainModel?> GetByLearnerKey(Guid key)
    {
        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.EnglishAndMathsCourses).ThenInclude(y => y.BreaksInLearning)
            .Include(x => x.Episodes).ThenInclude(y => y.Prices)
            .Include(x => x.Episodes).ThenInclude(y => y.LearningSupport)
            .Include(x => x.Episodes).ThenInclude(y => y.BreaksInLearning)
            .SingleOrDefaultAsync(x => x.LearnerKey == key);

        if(apprenticeship == null)
            return null;

        return _learningFactory.GetExisting(apprenticeship);
    }

    public async Task<List<ApprenticeshipLearningDomainModel>> GetAllByLearnerKey(Guid learnerKey, long? ukprn = null, string? courseCode = null)
    {
        var query = DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.EnglishAndMathsCourses).ThenInclude(y => y.BreaksInLearning)
            .Include(x => x.Episodes).ThenInclude(y => y.Prices)
            .Include(x => x.Episodes).ThenInclude(y => y.LearningSupport)
            .Include(x => x.Episodes).ThenInclude(y => y.BreaksInLearning)
            .Where(x => x.LearnerKey == learnerKey);

        if (ukprn.HasValue)
            query = query.Where(x => x.Episodes.Any(e => e.Ukprn == ukprn.Value));

        if (courseCode != null)
            query = query.Where(x => x.TrainingCode == courseCode);

        var apprenticeships = await query.ToListAsync();

        return apprenticeships.Select(_learningFactory.GetExisting).ToList();
    }

    public async Task<List<ApprenticeshipLearningDomainModel>> GetOtherUnapprovedCourseLearnings(Guid learnerKey, long ukprn, string excludingCourseCode)
    {
        var apprenticeships = await DbContext.ApprenticeshipLearningDbSet
            .Include(x => x.EnglishAndMathsCourses).ThenInclude(y => y.BreaksInLearning)
            .Include(x => x.Episodes).ThenInclude(y => y.Prices)
            .Include(x => x.Episodes).ThenInclude(y => y.LearningSupport)
            .Include(x => x.Episodes).ThenInclude(y => y.BreaksInLearning)
            .Where(x => x.LearnerKey == learnerKey &&
                        x.TrainingCode != excludingCourseCode &&
                        x.Episodes.Any(e =>
                            e.Ukprn == ukprn &&
                            !e.IsApproved &&
                            !e.IsRemoved))
            .ToListAsync();

        return apprenticeships.Select(_learningFactory.GetExisting).ToList();
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
                        x.Episodes.Any(e => e.ApprovalsApprenticeshipId == approvalsApprenticeshipId))
            .Include(x => x.EnglishAndMathsCourses)
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
            .Include(x => x.EnglishAndMathsCourses)
            .Include(x => x.Episodes)
                .ThenInclude(e => e.Prices)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return apprenticeship == null
            ? null
            : _learningFactory.GetExisting(apprenticeship);
    }

    public Task Update(ApprenticeshipLearningDomainModel learning)
    {
        _unitOfWork.Track(learning);
        return Task.CompletedTask;
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

    async Task<LearningDomainModel?> ILearningRepository.GetUnapprovedLearning(string uln, long apprenticeshipId, string? trainingCode = null)
    {
        // Nb, cannot look this up by apprenticeshipId - a draft's ApprovalsApprenticeshipId is hard-coded to 0
        // until approval, so the real (incoming) apprenticeshipId never matches the draft it's meant to approve.
        var learnerKey = await DbContext.LearnersDbSet
            .Where(l => l.Uln == uln)
            .Select(l => l.Key)
            .SingleOrDefaultAsync();

        if (learnerKey == default)
            return null;

        // Nb, not AsNoTracking - UnitOfWork.Track() just queues this aggregate for event dispatch,
        // it doesn't attach/re-track the entity. Persisting a later Approve() call depends on this
        // query leaving EF's change tracker holding the entity, same as ShortCourseLearningRepository.Get.
        var apprenticeship = await DbContext.ApprenticeshipLearningDbSet
            .Where(x => x.LearnerKey == learnerKey &&
                        x.TrainingCode == trainingCode &&
                        x.Episodes.Any(e => !e.IsApproved))
            .Include(x => x.EnglishAndMathsCourses)
            .Include(x => x.Episodes)
                .ThenInclude(e => e.Prices)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        return apprenticeship == null
            ? null
            : _learningFactory.GetExisting(apprenticeship);
    }

}