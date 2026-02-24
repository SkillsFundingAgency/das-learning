using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public abstract class LearningSupportDomainModel<T> where T : LearningSupport
{
    private readonly T _entity;
    public Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;

    internal LearningSupportDomainModel(T entity)
    {
        _entity = entity;
    }
}

public class ApprenticeshipLearningSupportDomainModel : LearningSupportDomainModel<ApprenticeshipLearningSupport>
{
    public ApprenticeshipLearningSupportDomainModel(ApprenticeshipLearningSupport entity) : base(entity) { }

    public static ApprenticeshipLearningSupportDomainModel Get(ApprenticeshipLearningSupport entity)
    {
        return new ApprenticeshipLearningSupportDomainModel(entity);
    }
}

public class ShortCourseLearningSupportDomainModel : LearningSupportDomainModel<ShortCourseLearningSupport>
{
    public ShortCourseLearningSupportDomainModel(ShortCourseLearningSupport entity) : base(entity) { }

    public static ShortCourseLearningSupportDomainModel Get(ShortCourseLearningSupport entity)
    {
        return new ShortCourseLearningSupportDomainModel(entity);
    }
}