using SFA.DAS.Learning.Domain.Models.Shared;

namespace SFA.DAS.Learning.Domain.Models.Apprenticeships;

#pragma warning disable CS8618 // Required properties must be set in the constructor

public class LearnerUpdateModel
{
    public DeliveryDetails Delivery { get; set; }
    public LearningUpdateDetails Learning { get; set; }
    public List<MathsAndEnglishUpdateDetails> MathsAndEnglishCourses { get; set; }
    public List<LearningSupportDetails> LearningSupport { get; set; }
    public OnProgrammeDetails OnProgrammeDetails { get; set; }
}

public class DeliveryDetails
{
    public DateTime? WithdrawalDate { get; set; }
}

//todo on the ShortCourses tech design the Learner object is shared between Apprenticeships and ShortCourses without any inheritance, however these two fields are not on the design but are currently required for Apprenticeships.
//Suspect these should be refactored to live on the Apprenticeship specific learning delivery, and we do away with the inheritance and just use the shared Learner object.
public class LearningUpdateDetails : Learner
{
    public DateTime? CompletionDate { get; set; }
    public CareDetails Care { get; set; }
}

public class MathsAndEnglishUpdateDetails
{
    public string Course { get; set; }
    public string LearnAimRef { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public int? PriorLearningPercentage { get; set; }
    public decimal Amount { get; set; }
    public List<BreakInLearningUpdateDetails> BreaksInLearning { get; set; }
}

public class OnProgrammeDetails
{
    public DateTime ExpectedEndDate { get; set; }
    public List<Cost> Costs { get; set; }
    public DateTime? PauseDate { get; set; }
    public List<BreakInLearningUpdateDetails> BreaksInLearning { get; set; }
}

public class Cost
{
    public int TrainingPrice { get; set; }

    public int? EpaoPrice { get; set; }

    public DateTime FromDate { get; set; }

    public int TotalPrice => TrainingPrice + (EpaoPrice ?? 0);
}

public class BreakInLearningUpdateDetails
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}

public class CareDetails
{
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }
}

#pragma warning restore CS8618