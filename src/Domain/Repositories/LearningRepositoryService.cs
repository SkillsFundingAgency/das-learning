using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;


namespace SFA.DAS.Learning.Domain.Repositories
{
    public interface ILearningRepository
    {
        Task<LearningDomainModel?> GetLearning(string uln, long apprenticeshipId);
        Task AddLearning(LearningDomainModel model);
        Task UpdateLearning(LearningDomainModel model);
    }

    public interface ILearningRepositoryProvider
    {
        ILearningRepository GetRepository(LearningType type);
    }

    public class LearningRepositoryProvider : ILearningRepositoryProvider
    {
        private readonly IApprenticeshipLearningRepository _apprenticeships;
        private readonly IShortCourseLearningRepository _shortCourses;

        public LearningRepositoryProvider(
            IApprenticeshipLearningRepository apprenticeships,
            IShortCourseLearningRepository shortCourses)
        {
            _apprenticeships = apprenticeships;
            _shortCourses = shortCourses;
        }

        public ILearningRepository GetRepository(LearningType type) =>
            type switch
            {
                LearningType.Apprenticeship => _apprenticeships,
                LearningType.FoundationApprenticeship => _apprenticeships,
                LearningType.ApprenticeshipUnit => _shortCourses,
                _ => throw new NotSupportedException($"Unsupported learning type: {type}")
            };
    }

    public interface ILearningService
    {
        Task<LearningDomainModel?> GetLearning(
            string uln,
            LearningType type,
            bool isApproved,
            long approvalsApprenticeshipId);

        Task AddLearning(LearningDomainModel model, LearningType type);

        Task UpdateLearning(LearningDomainModel model, LearningType type);
    }

    public class LearningService : ILearningService
    {
        private readonly ILearningRepositoryProvider _provider;

        public LearningService(ILearningRepositoryProvider provider)
        {
            _provider = provider;
        }

        public Task<LearningDomainModel?> GetLearning(
            string uln,
            LearningType type,
            bool isApproved,
            long approvalsApprenticeshipId)
        {
            var repo = _provider.GetRepository(type);
            return repo.GetLearning(uln, approvalsApprenticeshipId);
        }

        public Task AddLearning(LearningDomainModel model, LearningType type)
        {
            var repo = _provider.GetRepository(type);
            return repo.AddLearning(model);
        }

        public Task UpdateLearning(LearningDomainModel model, LearningType type)
        {
            var repo = _provider.GetRepository(type);
            return repo.UpdateLearning(model);
        }
    }


    //public interface ILearningRepositoryService
    //{
    //    Task<ILearningDomainModel?> Get(string uln, LearningType learningType, bool isApproved, long approvalsApprenticeshipId);
    //    Task SaveChanges();
    //}

    //public class LearningRepositoryService(IApprenticeshipLearningRepository apprenticeshipLearningRepository, IShortCourseLearningRepository shortCourseLearningRepository, Lazy<LearningDataContext> dbContext) : ILearningRepositoryService
    //{
    //    public async Task<ILearningDomainModel?> Get(string uln, LearningType learningType, bool isApproved, long approvalsApprenticeshipId)
    //    {
    //        if (learningType is LearningType.Apprenticeship or LearningType.FoundationApprenticeship)
    //        {
    //            return await apprenticeshipLearningRepository.Get(uln, approvalsApprenticeshipId); //todo: check if isApproved - add another parameter maybe?
    //        }

    //        //return await shortCourseLearningRepository.Get(uln, isApproved); //todo: add this method

    //        throw new NotImplementedException();

    //    }

    //    public async Task SaveChanges()
    //    {
    //        await dbContext.Value.SaveChangesAsync();
    //    }
    //}
}
