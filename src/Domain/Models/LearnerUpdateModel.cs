namespace SFA.DAS.Learning.Domain.Models;

public class LearnerUpdateModel
{
    public LearningUpdateDetails Learning { get; set; }
    public List<MathsAndEnglishUpdateDetails> MathsAndEnglishCourses { get; set; }
    public List<LearningSupportDetails> LearningSupport { get; set; }
    public OnProgrammeDetails OnProgrammeDetails { get; set; }
}

public class LearningUpdateDetails
{
    public DateTime? CompletionDate { get; set; }
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
}

public class LearningSupportDetails
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class OnProgrammeDetails
{
    public List<Cost> Costs { get; set; }
}

public class Cost
{
    public int TrainingPrice { get; set; }

    public int? EpaoPrice { get; set; }

    public DateTime FromDate { get; set; }

    public int TotalPrice => TrainingPrice + (EpaoPrice ?? 0);
}
