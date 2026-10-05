using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

/// <summary>
/// Details needed to create a draft apprenticeship learning in its full state.
/// </summary>
public class DraftApprenticeshipDetails
{
    public long ApprovalsApprenticeshipId { get; init; }
    public string LearnerRef { get; init; } = string.Empty;
    public LearningType LearningType { get; init; } = LearningType.Apprenticeship;

    /// <summary>The single (earliest) cost the draft is created with.</summary>
    public required Cost Cost { get; init; }
    public required DateTime ExpectedEndDate { get; init; }

    public DateTime? WithdrawalDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public DateTime? AchievementDate { get; init; }
    public DateTime? PauseDate { get; init; }

    public List<BreakInLearningUpdateDetails> BreaksInLearning { get; init; } = [];
    public List<LearningSupportDetails> LearningSupport { get; init; } = [];
    public List<EnglishAndMathsUpdateDetails> EnglishAndMathsCourses { get; init; } = [];
}
