using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsResponse(IEnumerable<LearnerSummary> learnings)
{
    public IEnumerable<LearnerSummary> Learnings { get; set; } = learnings;
}

[ExcludeFromCodeCoverage]
public class LearnerSummary
{
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
