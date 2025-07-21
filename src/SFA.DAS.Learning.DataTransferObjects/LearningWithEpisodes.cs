using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.DataTransferObjects;

[ExcludeFromCodeCoverage]
public class LearningWithEpisodes
{
    public LearningWithEpisodes(
        Guid key, 
        string uln, 
        DateTime startDate, 
        DateTime plannedEndDate, 
        List<Episode> episodes, 
        int ageAtStartOfLearning, 
        DateTime? withdrawnDate,
        DateTime? completionDate)
    {
        Key = key;
        Uln = uln;
        StartDate = startDate;
        PlannedEndDate = plannedEndDate;
        Episodes = episodes;
        AgeAtStartOfLearning = ageAtStartOfLearning;
        WithdrawnDate = withdrawnDate;
        CompletionDate = completionDate;
    }

    public Guid Key { get; set; }
    public string Uln { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public List<Episode> Episodes { get; set; }
    public int AgeAtStartOfApprenticeship => AgeAtStartOfLearning;
    public int AgeAtStartOfLearning { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}