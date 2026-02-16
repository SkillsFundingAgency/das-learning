namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsResponse(IEnumerable<Models.Learning> learnings)
{
    public IEnumerable<Models.Learning> Learnings { get; set; } = learnings;
}
