namespace SFA.DAS.Learning.Domain.Models;

public class LearnerUpdateModel
{
    public LearningUpdateDetails Learning { get; }
    public List<MathsAndEnglishUpdateDetails> MathsAndEnglishCourses { get; }

    public LearnerUpdateModel(LearningUpdateDetails learning, List<MathsAndEnglishUpdateDetails> mathsAndEnglishCourses)
    {
        Learning = learning;
        MathsAndEnglishCourses = mathsAndEnglishCourses;
    }
}

public class LearningUpdateDetails
{
    public DateTime? CompletionDate { get; set; }

    public LearningUpdateDetails(DateTime? completionDate)
    {
        CompletionDate = completionDate;
    }
}

public class MathsAndEnglishUpdateDetails
{
    public string Course { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public int? PriorLearningPercentage { get; set; }

    public MathsAndEnglishUpdateDetails(
        string course,
        DateTime startDate,
        DateTime plannedEndDate,
        DateTime? completionDate,
        DateTime? withdrawalDate,
        int? priorLearningPercentage)
    {
        Course = course;
        StartDate = startDate;
        PlannedEndDate = plannedEndDate;
        CompletionDate = completionDate;
        WithdrawalDate = withdrawalDate;
        PriorLearningPercentage = priorLearningPercentage;
    }
}