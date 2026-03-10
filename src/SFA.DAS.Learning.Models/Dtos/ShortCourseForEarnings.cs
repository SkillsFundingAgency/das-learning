using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Models.Dtos;

[ExcludeFromCodeCoverage]
public class ShortCourseForEarnings
{
    public Guid LearningKey { get; set; }
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<ShortCourseForEarningEpisode> Episodes { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class ShortCourseForEarningEpisode
{
    public string CourseCode { get; set; }
    public bool IsApproved { get; set; }
    public decimal Price { get; set; }
}
