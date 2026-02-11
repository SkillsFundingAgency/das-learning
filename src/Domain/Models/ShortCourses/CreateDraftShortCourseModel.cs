using SFA.DAS.Learning.Domain.Models.Shared;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Models.ShortCourses;

public class CreateDraftShortCourseModel
{
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
}