namespace SFA.DAS.Learning.Domain.Models.Shared;

#pragma warning disable CS8618
public class LearningSupportDetails
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class Learner
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime DateOfBirth { get; set; }
}