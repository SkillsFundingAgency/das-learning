namespace SFA.DAS.Learning.Domain.Models;

public class LearnerUpdateModel
{
    public LearnerUpdateDetails Learner { get; }

    public LearnerUpdateModel(LearnerUpdateDetails learner)
    {
        Learner = learner;
    }
}

public class LearnerUpdateDetails
{
    public DateTime? CompletionDate { get; }

    public LearnerUpdateDetails(DateTime? completionDate)
    {
        CompletionDate = completionDate;
    }
}