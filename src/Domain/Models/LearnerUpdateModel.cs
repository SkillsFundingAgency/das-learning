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
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public string Course { get; set; }

    public MathsAndEnglishUpdateDetails(DateTime? completionDate, DateTime? withdrawalDate, string course)
    {
        CompletionDate = completionDate;
        WithdrawalDate = withdrawalDate;
        Course = course;
    }
}