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
        var learnerDetails = new Domain.Models.LearnerUpdateDetails(request.Learner.CompletionDate);
        var learnerUpdateModel = new LearnerUpdateModel(learnerDetails);
        return new UpdateLearnerCommand(learnerKey, learnerUpdateModel);
    }
}