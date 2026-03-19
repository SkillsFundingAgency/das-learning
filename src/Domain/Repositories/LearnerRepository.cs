using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;

namespace SFA.DAS.Learning.Domain.Repositories;

public class LearnerRepository : ILearnerRepository
{
    private readonly Lazy<LearningDataContext> _lazyContext;
    private IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILearnerFactory _learnerFactory;
    private LearningDataContext DbContext => _lazyContext.Value;

    private readonly IUnitOfWork _unitOfWork;

    public LearnerRepository(Lazy<LearningDataContext> dbContext, IDomainEventDispatcher domainEventDispatcher, ILearnerFactory learnerFactory, IUnitOfWork unitOfWork)
    {
        _lazyContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _learnerFactory = learnerFactory;
        _unitOfWork = unitOfWork;
    }


    public async Task Add(LearnerDomainModel learner)
    {
        var entity = learner.GetEntity();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();

        foreach (dynamic domainEvent in learner.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task<LearnerDomainModel?> Get(Guid learnerKey)
    {
        var learner = await DbContext.LearnersDbSet.FirstOrDefaultAsync(x => x.Key == learnerKey);

        if (learner == null)
        {
            return null;
        }

        return _learnerFactory.GetExisting(learner);
    }

    public async Task<LearnerDomainModel> GetByLearningKey(Guid learningKey)
    {
        var learning = await DbContext.ApprenticeshipLearningDbSet.SingleAsync(x => x.Key == learningKey);
        var learner = await DbContext.LearnersDbSet.SingleAsync(x => x.Key == learning.LearnerKey); 
        return _learnerFactory.GetExisting(learner);
    }

    public async Task<LearnerDomainModel?> GetByUln(string uln)
    {
        var learner = await DbContext.LearnersDbSet.FirstOrDefaultAsync(x => x.Uln == uln);

        if (learner == null)
        {
            return null;
        }

        return _learnerFactory.GetExisting(learner);
    }

    public Task Update(LearnerDomainModel learner)
    {
        _unitOfWork.Track(learner);
        return Task.CompletedTask;
    }
}
