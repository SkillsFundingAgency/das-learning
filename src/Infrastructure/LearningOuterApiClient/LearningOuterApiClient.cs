using System.Diagnostics.CodeAnalysis;
using System.Net;
using Newtonsoft.Json;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Calendar;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;

[ExcludeFromCodeCoverage]
public class LearningOuterApiClient : ILearningOuterApiClient
{
    private readonly HttpClient _httpClient;

    private const string GetStandardUrl = "TrainingCourses/standards";
    private const string GetAcademicYearUrl = "CollectionCalendar/academicYear";

    public LearningOuterApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetStandardResponse> GetStandard(int courseCode)
    {
        var response = await _httpClient.GetAsync($"{GetStandardUrl}/{courseCode}").ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            throw new Exception("Standard not found.");
        }

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GetStandardResponse>(json);
        }

        throw new Exception($"Status code: {response.StatusCode} returned from apprenticeships outer api.");
    }

    public async Task<GetAcademicYearsResponse> GetAcademicYear(DateTime searchYear)
    {
        var response = await _httpClient.GetAsync($"{GetAcademicYearUrl}/{searchYear.ToString("yyyy-MM-dd")}").ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            throw new Exception("Academic year not found.");
        }

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<GetAcademicYearsResponse>(json);
        }

        throw new Exception($"Status code: {response.StatusCode} returned from apprenticeships outer api.");
    }

}