using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ApprenticeshipLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipLearning")]
public class ApprenticeshipLearning : Learning
{
    public ApprenticeshipLearning()
    {
        Episodes = new List<ApprenticeshipEpisode>();
        EnglishAndMathsCourses = new List<EnglishAndMaths>();
    }
    public List<ApprenticeshipEpisode> Episodes { get; set; }
    public List<EnglishAndMaths> EnglishAndMathsCourses { get; set; }
    public LearningType LearningType { get; set; }
    public string TrainingCode { get; set; } = null!;
    public string? TrainingCourseVersion { get; set; }
}