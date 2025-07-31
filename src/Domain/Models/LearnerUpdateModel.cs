namespace SFA.DAS.Learning.Domain.Models;

public class LearnerUpdateModel
{
    public LearningUpdateDetails Learning { get; }
    public List<MathsAndEnglishUpdateDetails> MathsAndEnglishCourses { get; }
    public List<LearningSupportDetails> LearningSupport { get; }

    public LearnerUpdateModel(
        LearningUpdateDetails learning, 
        List<MathsAndEnglishUpdateDetails> mathsAndEnglishCourses,
        List<LearningSupportDetails> learningSupport
        )
    {
        Learning = learning;
        MathsAndEnglishCourses = mathsAndEnglishCourses;
        LearningSupport = learningSupport;
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
    public decimal Amount { get; set; }

    public MathsAndEnglishUpdateDetails(
        string course,
        DateTime startDate,
        DateTime plannedEndDate,
        DateTime? completionDate,
        DateTime? withdrawalDate,
        int? priorLearningPercentage,
        decimal amount)
    {
        Course = course;
        StartDate = startDate;
        PlannedEndDate = plannedEndDate;
        CompletionDate = completionDate;
        WithdrawalDate = withdrawalDate;
        PriorLearningPercentage = priorLearningPercentage;
        Amount = amount;
    }
}

public class LearningSupportDetails
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LearningSupportDetails(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}