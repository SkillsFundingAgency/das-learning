using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.InnerApi.Requests.ShortCourses;

#pragma warning disable CS8618
/// <summary>
/// Request to create a draft short course learner record
/// </summary>
public class CreateDraftShortCourseRequest
{
    /// <summary>
    /// Learner details to be updated
    /// </summary>
    public ShortCourseLearnerUpdateDetails LearnerUpdateDetails { get; set; }

    /// <summary>
    /// Learning support details
    /// </summary>
    public List<Requests.Shared.LearningSupportDetails> LearningSupport { get; set; } = new();

    /// <summary>
    /// On programme details
    /// </summary>
    public List<OnProgramme> OnProgramme { get; set; } = new();
}

/// <summary>
/// On programme details
/// </summary>
public class OnProgramme
{
    /// <summary>
    /// Course code
    /// </summary>
    public string CourseCode { get; set; } = null!;

    /// <summary>
    /// Employer identifier
    /// </summary>
    public long EmployerId { get; set; }

    /// <summary>
    /// Provider UKPRN
    /// </summary>
    public long Ukprn { get; set; }

    /// <summary>
    /// Start date of the short course
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Withdrawal date of the short course
    /// </summary>
    public DateTime? WithdrawalDate { get; set; }

    /// <summary>
    /// Withdrawal reason code
    /// </summary>
    public short? WithdrawalReasonCode { get; set; }

    /// <summary>
    /// Completion date of the short course
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Expected end date of the short course
    /// </summary>
    public DateTime ExpectedEndDate { get; set; }

    /// <summary>
    /// Milestones for the short course
    /// </summary>
    public List<Milestone> Milestones { get; set; } = new();

    /// <summary>
    /// Price of the short course
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Learning type of the short course
    /// </summary>
    public LearningType LearningType { get; set; }
}

/// <summary>
/// Request to update an existing short course learner record with an array of OnProgramme items.
/// </summary>
public class UpdateShortCourseRequest
{
    public long Ukprn { get; set; }
    public ShortCourseLearnerUpdateDetails LearnerUpdateDetails { get; set; }
    public List<OnProgramme> OnProgramme { get; set; } = new();
}

public static class UpdateShortCourseRequestExtensions
{
    public static List<ShortCourseUpdateContext> ToUpdateModels(this UpdateShortCourseRequest request)
    {
        return request.OnProgramme.Select(op => new ShortCourseUpdateContext
        {
            LearnerRef = request.LearnerUpdateDetails.LearnerRef,
            Learner = new LearnerModel
            {
                FirstName = request.LearnerUpdateDetails.FirstName,
                LastName = request.LearnerUpdateDetails.LastName,
                EmailAddress = request.LearnerUpdateDetails.EmailAddress,
                DateOfBirth = request.LearnerUpdateDetails.DateOfBirth,
                Uln = request.LearnerUpdateDetails.Uln.ToString()
            },
            LearningSupport = new List<Learning.Models.UpdateModels.Shared.LearningSupportDetails>(),
            OnProgramme = new Learning.Models.UpdateModels.OnProgramme
            {
                CourseCode = op.CourseCode,
                EmployerId = op.EmployerId,
                Ukprn = op.Ukprn,
                StartDate = op.StartDate,
                WithdrawalDate = op.WithdrawalDate,
                WithdrawalReasonCode = op.WithdrawalReasonCode,
                CompletionDate = op.CompletionDate,
                ExpectedEndDate = op.ExpectedEndDate,
                Milestones = op.Milestones ?? new List<Milestone>(),
                Price = op.Price,
                LearningType = op.LearningType
            }
        }).ToList();
    }
}

/// <summary>
/// CreateDraftShortCourse request extensions
/// </summary>
public static class CreateDraftShortCourseRequestExtensions
{
    /// <summary>
    /// Maps a <see cref="CreateDraftShortCourseRequest"/> to a list of <see cref="ShortCourseUpdateContext"/>, one per OnProgramme item.
    /// </summary>
    /// <param name="request">The request to map.</param>
    /// <returns>A new list of <see cref="ShortCourseUpdateContext"/>.</returns>
    public static List<ShortCourseUpdateContext> ToCreateModels(this CreateDraftShortCourseRequest request)
    {
        return request.OnProgramme.Select(op => new ShortCourseUpdateContext
        {
            LearnerRef = request.LearnerUpdateDetails.LearnerRef,
            Learner = new LearnerModel
            {
                FirstName = request.LearnerUpdateDetails.FirstName,
                LastName = request.LearnerUpdateDetails.LastName,
                EmailAddress = request.LearnerUpdateDetails.EmailAddress,
                DateOfBirth = request.LearnerUpdateDetails.DateOfBirth,
                Uln = request.LearnerUpdateDetails.Uln.ToString()
            },
            LearningSupport = request.LearningSupport.SelectOrEmptyList(x =>
                new Learning.Models.UpdateModels.Shared.LearningSupportDetails
                {
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }),
            OnProgramme = new Learning.Models.UpdateModels.OnProgramme
            {
                CourseCode = op.CourseCode,
                EmployerId = op.EmployerId,
                Ukprn = op.Ukprn,
                StartDate = op.StartDate,
                WithdrawalDate = op.WithdrawalDate,
                WithdrawalReasonCode = op.WithdrawalReasonCode,
                CompletionDate = op.CompletionDate,
                ExpectedEndDate = op.ExpectedEndDate,
                Milestones = op.Milestones ?? new List<Milestone>(),
                Price = op.Price,
                LearningType = op.LearningType
            }
        }).ToList();
    }
}