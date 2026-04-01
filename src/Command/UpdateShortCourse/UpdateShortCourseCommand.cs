using System.Text.Json.Serialization;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommand : ICommand
{
    public Guid LearningKey { get; }
    public ShortCourseUpdateContext Model { get; }

    public UpdateShortCourseCommand(Guid learningKey, ShortCourseUpdateContext model)
    {
        LearningKey = learningKey;
        Model = model;
    }
}

public class UpdateShortCourseResult
{
    public Guid LearningKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public ShortCourseUpdateChanges[] Changes { get; set; } = [];
    public UpdateShortCourseResultLearner Learner { get; set; } = null!;
    public UpdateShortCourseResultEpisode[] Episodes { get; set; } = [];
}

public class UpdateShortCourseResultLearner
{
    public string Uln { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class UpdateShortCourseResultEpisode
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
}
