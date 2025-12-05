using System.Diagnostics.CodeAnalysis;
using System.Net;
using Newtonsoft.Json;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Calendar;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;

[ExcludeFromCodeCoverage]
public class LearningOuterApiClient : ILearningOuterApiClient
{
    private readonly HttpClient _httpClient;

    private const string GetAcademicYearUrl = "CollectionCalendar/academicYear";

    public LearningOuterApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetAcademicYearsResponse> GetAcademicYear(DateTime searchYear)
    {
        var response = await _httpClient.GetAsync($"{GetAcademicYearUrl}/{searchYear.ToString("yyyy-MM-dd")}").ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            throw new Exception("Academic year not found.");
        }

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Status code: {response.StatusCode} returned from apprenticeships outer api.");

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var academicYearResponse = JsonConvert.DeserializeObject<GetAcademicYearsResponse>(json);
        
        if(academicYearResponse == null)
            throw new Exception("Academic year response was null.");

        return academicYearResponse;

    }

}