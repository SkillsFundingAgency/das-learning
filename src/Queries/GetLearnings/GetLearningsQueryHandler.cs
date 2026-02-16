using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Queries.GetLearnings;

public class GetLearningsQueryHandler : IQueryHandler<GetLearningsRequest, GetLearningsResponse>
{
    private readonly ILearningQueryRepository _learningQueryRepository;

    public GetLearningsQueryHandler(ILearningQueryRepository learningQueryRepository)
    {
        _learningQueryRepository = learningQueryRepository;
    }

    public async Task<GetLearningsResponse> Handle(GetLearningsRequest query, CancellationToken cancellationToken = default)
    {
        var apprenticeships = await _learningQueryRepository.GetAll(query.Ukprn, query.FundingPlatform);

        var response = new GetLearningsResponse(apprenticeships);

        return await Task.FromResult(response);
    }
}