using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Services;

public class LearningRepositoryProvider(
    IApprenticeshipLearningRepository apprenticeships,
    IShortCourseLearningRepository shortCourses)
    : ILearningRepositoryProvider
{
    public ILearningRepository GetRepository(LearningType type) =>
        type switch
        {
            LearningType.Apprenticeship => apprenticeships,
            LearningType.FoundationApprenticeship => apprenticeships,
            LearningType.ApprenticeshipUnit => shortCourses,
            _ => throw new NotSupportedException($"Unsupported learning type: {type}")
        };

    public ILearningRepository GetRepository(LearningDomainModel model) =>
        model switch
        {
            ApprenticeshipLearningDomainModel => apprenticeships,
            ShortCourseLearningDomainModel => shortCourses,
            _ => throw new NotSupportedException($"Unsupported domain model type: {model.GetType().Name}")
        };
}