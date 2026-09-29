using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsRequest : IQuery
{
    public long Ukprn { get; }

    public GetLearningsRequest(long ukprn)
    {
        Ukprn = ukprn;
    }
}
