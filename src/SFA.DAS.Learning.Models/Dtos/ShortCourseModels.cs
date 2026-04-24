using SFA.DAS.Learning.Enums;
using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Models.Dtos;

#pragma warning disable CS8618
public abstract class ShortCourseLearningDto
{
    public Guid LearningKey { get; set; }
    public Guid LearnerKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseLearnerDto Learner { get; set; }
    public ShortCourseEpisodeDto[] Episodes { get; set; } = [];
}

public class ShortCourseLearnerDto
{
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class ShortCourseEpisodeDto
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
    public long? TransferSenderId { get; set; }
}

#pragma warning restore CS8618
