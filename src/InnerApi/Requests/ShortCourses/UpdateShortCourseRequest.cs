using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

#pragma warning disable CS8618
public class UpdateShortCourseRequest
{
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
}
#pragma warning restore CS8618

public static class UpdateShortCourseRequestExtensions
{
    public static ShortCourseUpdateContext ToUpdateModel(this UpdateShortCourseRequest request)
    {
        return new ShortCourseUpdateContext
        {
            LearningSupport = new List<LearningSupportDetails>(),
            OnProgramme = new OnProgramme
            {
                WithdrawalDate = request.WithdrawalDate,
                CompletionDate = request.CompletionDate,
                Milestones = request.Milestones ?? new List<Milestone>()
            }
        };
    }
}
