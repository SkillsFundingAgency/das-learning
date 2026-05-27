using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

#pragma warning disable CS8618 // Required properties must be set in the constructor

/// <summary>
/// Request to update a learner's details
/// </summary>
public class UpdateLearnerRequest: CreateDraftApprenticeship
{
    
}

/// <summary>
/// Learner details to be updated for Short Courses
/// </summary>
public class ShortCourseLearnerUpdateDetails : LearnerUpdateDetails
{
    /// <summary> Uln</summary>
    public long Uln { get; set; }
    /// <summary> Learner Reference </summary>
    public string LearnerRef { get; set; }
}

#pragma warning restore CS8618 // Required properties must be set in the constructor

/// <summary>
/// Extension methods for updating learner requests
/// </summary>
public static class UpdateLearnerRequestExtensions
{
    /// <summary>
    /// Converts the request to a command for updating a learner
    /// </summary>
    /// <param name="request">The request containing learner details</param>
    /// <returns>A command to update the learner</returns>
    public static LearningUpdateContext ToUpdateModel(this UpdateLearnerRequest request, Guid learningKey)
    {
        var updateContext = CreateDraftApprenticeshipExtensions.ToUpdateModel(request);
        updateContext.LearningKey = learningKey;
        return updateContext;
    }

}