namespace SFA.DAS.Learning.InnerApi.Requests.Shared;

#pragma warning disable CS8618
/// <summary>
/// Learner details to be updated
/// </summary>
public class  LearnerUpdateDetails
{
    /// <summary>
    /// The first or given names of the learner
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the learner
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// The email address of the learner
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Date of birth of the learner
    /// </summary>
    public DateTime DateOfBirth { get; set; }
}