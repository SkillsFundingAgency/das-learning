namespace SFA.DAS.Learning.Models.Dtos;

#pragma warning disable CS8618
public class ShortCourseLearner
{
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class ShortCourseEpisode
{
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public string CourseCode { get; set; } = null!;
    public string CourseType { get; set; } = null!;
    public string LearningType { get; set; }
    public DateTime StartDate { get; set; }
    public int AgeAtStart { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public bool IsApproved { get; set; }
    public decimal Price { get; set; }
    public string LearnerRef { get; set; } = null!;
}

#pragma warning restore CS8618
