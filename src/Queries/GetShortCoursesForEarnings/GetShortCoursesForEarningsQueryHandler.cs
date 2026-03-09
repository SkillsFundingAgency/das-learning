using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Queries.GetShortCoursesForEarnings;

public class GetShortCoursesForEarningsQueryHandler(
    IShortCourseLearningRepository repository)
    : IQueryHandler<GetShortCoursesForEarningsRequest, GetShortCoursesForEarningsResponse>
{
    public async Task<GetShortCoursesForEarningsResponse> Handle(GetShortCoursesForEarningsRequest query, CancellationToken cancellationToken = default)
    {
        var dates = AcademicYearParser.ParseFrom(query.CollectionYear);

        var response = await repository.GetForEarnings(
            query.UkPrn,
            dates,
            query.Limit,
            query.Offset,
            cancellationToken);

        return new GetShortCoursesForEarningsResponse
        {
            Items = response.Data.Select(x => new GetShortCoursesForEarningsItem
            {
                LearningKey = x.LearningKey,
                Learner = new GetShortCoursesForEarningsLearner
                {
                    Uln = x.Uln,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfBirth = x.DateOfBirth
                },
                Episodes = x.Episodes.Select(e => new GetShortCoursesForEarningsEpisode
                {
                    CourseCode = e.CourseCode,
                    IsApproved = e.IsApproved
                })
            }),
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = response.TotalItems
        };
    }
}
