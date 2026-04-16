using System.Text.Json.Serialization;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command;

public abstract class ShortCourseLearningResult
{
    public Guid LearningKey { get; set; }
    public Guid LearnerKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseLearningResultLearner Learner { get; set; } = null!;
    public ShortCourseLearningResultEpisode[] Episodes { get; set; } = [];
}

public class ShortCourseLearningResultLearner
{
    public string Uln { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class ShortCourseLearningResultEpisode
{
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public string CourseCode { get; set; } = null!;
    public string CourseType { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LearningType LearningType { get; set; }
    public DateTime StartDate { get; set; }
    public int AgeAtStart { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public bool IsApproved { get; set; }
    public decimal Price { get; set; }
    public string LearnerRef { get; set; } = null!;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EmployerType EmployerType { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
}
