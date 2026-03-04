namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsResponse(IEnumerable<Models.Dtos.Learning> learnings)
{
    public IEnumerable<Models.Dtos.Learning> Learnings { get; set; } = learnings;
}
