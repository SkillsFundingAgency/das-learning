using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Calendar;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;

public interface ILearningOuterApiClient
{
    Task<GetStandardResponse> GetStandard(int courseCode);
    Task<GetAcademicYearsResponse> GetAcademicYear(DateTime searchYear);
}