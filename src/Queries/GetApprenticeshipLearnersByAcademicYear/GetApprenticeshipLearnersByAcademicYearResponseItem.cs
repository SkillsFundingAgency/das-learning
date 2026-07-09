namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public record GetApprenticeshipLearnersByAcademicYearResponseItem
{
    public string Uln { get; init; }
    public Guid Key { get; set; }
}