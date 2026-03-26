using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Models.UpdateModels;

#pragma warning disable CS8618 // Required properties must be set in the constructor
public class ShortCourseUpdateContext
{
    public string LearnerRef { get; set; }
    public LearnerModel Learner { get; set; }
    public List<LearningSupportDetails> LearningSupport { get; set; }
    public OnProgramme OnProgramme { get; set; }
}

public class OnProgramme
{
    public string CourseCode { get; init; } = null!;
    public long EmployerId { get; init; }
    public long Ukprn { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? WithdrawalDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public DateTime ExpectedEndDate { get; init; }
    public List<Milestone> Milestones { get; set; }
    public decimal Price { get; init; }
    public LearningType LearningType { get; init; }
}
#pragma warning restore CS8618