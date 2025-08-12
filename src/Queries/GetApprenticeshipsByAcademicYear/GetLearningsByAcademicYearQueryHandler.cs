using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public class GetLearningsByAcademicYearQueryHandler(
    ILearningQueryRepository queryRepository)
    : IQueryHandler<GetLearningsByAcademicYearRequest, GetLearningsByAcademicYearResponse>
{
    public async Task<GetLearningsByAcademicYearResponse> Handle(GetLearningsByAcademicYearRequest query, CancellationToken cancellationToken = default)
    {
        var academicYearDates = AcademicYearParser.ParseFrom(query.AcademicYear);
        
        var response  = await queryRepository.GetByDates(
            query.UkPrn,
            academicYearDates, 
            query.Limit,
            query.Offset,
            cancellationToken);

        return new GetLearningsByAcademicYearResponse
        {
            Items = response.Data.Select(apprenticeship => new GetLearningsByDatesResponseItem
            {
                Uln = apprenticeship.Uln,
                Key = apprenticeship.Key
            }),
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = response.TotalItems,
        };
    }
}