using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IApprenticeshipLearningFactory
{
    ApprenticeshipLearningDomainModel CreateNew(Guid learnerKey, string trainingCode, string? trainingCourseVersion = null, LearningType learningType = LearningType.Apprenticeship);

    ApprenticeshipLearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
}

public class ApprenticeshipLearningFactory : IApprenticeshipLearningFactory
{
    public ApprenticeshipLearningDomainModel CreateNew(Guid learnerKey, string trainingCode, string? trainingCourseVersion = null, LearningType learningType = LearningType.Apprenticeship)
    {
        return ApprenticeshipLearningDomainModel.New(learnerKey, trainingCode, trainingCourseVersion, learningType);
    }

    public ApprenticeshipLearningDomainModel GetExisting(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
    {
        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
