namespace SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

public class CheckApprovedApprenticeshipExistsRequest : IQuery
{
    public long Ukprn { get; }
    public string Uln { get; }
    public string TrainingCode { get; }
    public DateTime StartDate { get; }
    public bool IsApproved { get; }

    public CheckApprovedApprenticeshipExistsRequest(long ukprn, string uln, string trainingCode, DateTime startDate, bool isApproved)
    {
        Ukprn = ukprn;
        Uln = uln;
        TrainingCode = trainingCode;
        StartDate = startDate;
        IsApproved = isApproved;
    }
}
