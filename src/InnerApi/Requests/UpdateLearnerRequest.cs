using SFA.DAS.Learning.Command.UpdateLearner;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.InnerApi.Requests;

#pragma warning disable CS8618 // Required properties must be set in the constructor

/// <summary>
/// Request to update a learner's details
/// </summary>
public class UpdateLearnerRequest
{
    /// <summary>
    /// Learner details to be updated
    /// </summary>
    public LearnerUpdateDetails Learner { get; set; }


    /// <summary>
    /// Maths and English course details
    /// </summary>
    public List<MathsAndEnglish> MathsAndEnglishCourses { get; set; }
}

/// <summary>
/// Learner details to be updated
/// </summary>
public class  LearnerUpdateDetails
{
    /// <summary>
    /// Date the learning completes, this will be null until completion is confirmed
    /// </summary>
    public DateTime? CompletionDate { get; set; }
}

/// <summary>
/// Maths and English course details
/// </summary>
public class MathsAndEnglish
{
    /// <summary>
    /// The maths and english course
    /// </summary>
    public string Course { get; set; }

    /// <summary>
    /// Date the maths and english course completes, this will be null until completion is confirmed
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Withdrawal date for the maths and english course, this will be null until a withdrawal is confirmed
    /// </summary>
    public DateTime? WithdrawalDate { get; set; }

    /// <summary>
    /// Start date of the maths and english course
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Planned end date of the maths and english course
    /// </summary>
    public DateTime PlannedEndDate { get; set; }

}

#pragma warning restore CS8618 // Required properties must be set in the constructor

public static class UpdateLearnerRequestExtensions
{
    /// <summary>
    /// Converts the request to a command for updating a learner
    /// </summary>
    /// <param name="request">The request containing learner details</param>
    /// <param name="learnerKey">The unique identifier of the learner</param>
    /// <returns>A command to update the learner</returns>
    public static UpdateLearnerCommand ToCommand(this UpdateLearnerRequest request, Guid learnerKey)
    {
        var learningDetails = new Domain.Models.LearningUpdateDetails(request.Learner.CompletionDate);
        var mathsAndEnglishCourses = request.MathsAndEnglishCourses
            .Select(x => new MathsAndEnglishUpdateDetails(x.CompletionDate, x.WithdrawalDate, x.Course, x.StartDate, x.PlannedEndDate))
            .ToList();

        var learnerUpdateModel = new LearnerUpdateModel(learningDetails, mathsAndEnglishCourses);
        return new UpdateLearnerCommand(learnerKey, learnerUpdateModel);
    }
}