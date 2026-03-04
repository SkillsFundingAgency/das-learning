namespace SFA.DAS.Learning.Queries.GetShortCoursesByAcademicYear;

public record ShortCourseLearnerItem
{
    public string Uln { get; init; }
    public Guid Key { get; set; }
}
