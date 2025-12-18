using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Calendar;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;

public interface ILearningOuterApiClient
{
    Task<GetAcademicYearsResponse> GetAcademicYear(DateTime searchYear);
}