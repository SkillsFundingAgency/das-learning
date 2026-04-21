using System.Text.Json.Serialization;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

public class GetShortCoursesForEarningsItem
{
    public Guid LearningKey { get; set; }
    public GetShortCoursesForEarningsLearner Learner { get; set; }
    public IEnumerable<GetShortCoursesForEarningsEpisode> Episodes { get; set; }
}

public class GetShortCoursesForEarningsLearner
{
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class GetShortCoursesForEarningsEpisode
{
    public string CourseCode { get; set; }
    public bool IsApproved { get; set; }
    public decimal Price { get; set; }
    public string LearnerRef { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmployerType EmployerType { get; set; }
}
