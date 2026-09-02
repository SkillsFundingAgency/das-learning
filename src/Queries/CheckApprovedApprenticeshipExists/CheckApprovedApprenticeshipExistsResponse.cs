namespace SFA.DAS.Learning.Queries.CheckApprovedApprenticeshipExists;

public class CheckApprovedApprenticeshipExistsResponse(bool exists)
{
    public bool Exists { get; set; } = exists;
}
