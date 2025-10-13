using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Queries.GetLearningsWithEpisodes;

public class GetLearningsWithEpisodesRequestQueryHandler : IQueryHandler<GetLearningsWithEpisodesRequest, GetLearningsWithEpisodesResponse?>
{
    private readonly ILearningQueryRepository _learningQueryRepository;
    private readonly ILogger<GetLearningsWithEpisodesRequestQueryHandler> _logger;

    public GetLearningsWithEpisodesRequestQueryHandler(ILearningQueryRepository learningQueryRepository, ILogger<GetLearningsWithEpisodesRequestQueryHandler> logger)
    {
        _learningQueryRepository = learningQueryRepository;
        _logger = logger;
    }

    public async Task<GetLearningsWithEpisodesResponse?> Handle(GetLearningsWithEpisodesRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetLearningsWithEpisodesRequest for Ukprn: {ukprn} CollectionYear: {collectionYear} CollectionPeriod: {collectionPeriod} Pagination Limit: {limit} Pagination Offset: {offset}", query.Ukprn, query.CollectionYear, query.CollectionPeriod, query.Limit, query.Offset);

        var learnings = await _learningQueryRepository.GetLearningsWithEpisodes(query.Ukprn, query.CollectionYear.GetLastDay(query.CollectionPeriod), query.Limit, query.Offset);

        if (learnings?.Data == null || !learnings.Data.Any())
        {
            _logger.LogInformation("No learnings found for {ukprn} (Pagination Limit: {limit} Pagination Offset: {offset})", query.Ukprn, query.Limit, query.Offset);
            return null;
        }

        _logger.LogInformation("{numberFound} apprenticeships found for {ukprn} (Pagination Limit: {limit} Pagination Offset: {offset})", learnings.Data.Count(), query.Ukprn, query.Limit, query.Offset);
        return new GetLearningsWithEpisodesResponse
        {
            Items = learnings.Data,
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = learnings.TotalItems
        };
    }
}