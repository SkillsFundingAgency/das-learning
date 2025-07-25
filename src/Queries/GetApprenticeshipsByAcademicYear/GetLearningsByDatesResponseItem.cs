namespace SFA.DAS.Learning.Queries.GetApprenticeshipsByAcademicYear;

public record GetLearningsByDatesResponseItem
{
    public string Uln { get; init; }
    public Guid Key { get; set; }
}