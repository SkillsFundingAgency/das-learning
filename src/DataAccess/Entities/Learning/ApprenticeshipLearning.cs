namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ApprenticeshipLearning")]
[System.ComponentModel.DataAnnotations.Schema.Table("ApprenticeshipLearning")]
public class ApprenticeshipLearning : Learning
{
    public ApprenticeshipLearning()
    {
        Episodes = new List<ApprenticeshipEpisode>();
        MathsAndEnglishCourses = new List<MathsAndEnglish>();
    }
    public long ApprovalsApprenticeshipId { get; set; }
    public List<ApprenticeshipEpisode> Episodes { get; set; }
    public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; }
}