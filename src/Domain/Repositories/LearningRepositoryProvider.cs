using SFA.DAS.Learning.Enums;


namespace SFA.DAS.Learning.Domain.Repositories
{
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
}
