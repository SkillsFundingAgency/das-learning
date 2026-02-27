using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

public class GetShortCoursesByAcademicYearQueryHandler(
    IShortCourseLearningRepository repository)
    : IQueryHandler<GetShortCoursesByAcademicYearRequest, GetShortCoursesByAcademicYearResponse>
{
    public async Task<GetShortCoursesByAcademicYearResponse> Handle(GetShortCoursesByAcademicYearRequest query, CancellationToken cancellationToken = default)
    {
        var academicYearDates = AcademicYearParser.ParseFrom(query.AcademicYear);

        var response = await repository.GetApprovedByDates(
            query.UkPrn,
            academicYearDates,
            query.Limit,
            query.Offset,
            cancellationToken);

        return new GetShortCoursesByAcademicYearResponse
        {
            Items = response.Data.Select(shortCourse => new ShortCourseLearnerItem
            {
                Uln = shortCourse.Uln,
                Key = shortCourse.Key
            }),
            PageSize = query.Limit,
            Page = query.Page,
            TotalItems = response.TotalItems,
        };
    }
}
