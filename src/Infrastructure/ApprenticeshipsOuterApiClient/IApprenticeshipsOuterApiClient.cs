using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient.Calendar;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient.Standards;

namespace SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;

public interface IApprenticeshipsOuterApiClient
{
    Task<GetStandardResponse> GetStandard(int courseCode);
    Task<GetAcademicYearsResponse> GetAcademicYear(DateTime searchYear);
}