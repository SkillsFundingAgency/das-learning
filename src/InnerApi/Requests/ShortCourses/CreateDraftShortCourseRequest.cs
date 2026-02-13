using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Models.Shared;
using SFA.DAS.Learning.Domain.Models.ShortCourses;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

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
    public OnProgramme OnProgramme { get; set; }
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
}

public static class CreateDraftShortCourseRequestExtensions
{
    /// <summary>
    /// Maps a <see cref="CreateDraftShortCourseRequest"/> to a <see cref="CreateDraftShortCourseModel"/>.
    /// </summary>
    /// <param name="request">The request to map.</param>
    /// <returns>A new instance of <see cref="CreateDraftShortCourseModel"/>.</returns>
    public static CreateDraftShortCourseModel ToCreateModel(this CreateDraftShortCourseRequest request)
    {
        return new CreateDraftShortCourseModel
        {
            Learner = new LearnerModel
            {
                FirstName = request.LearnerUpdateDetails.FirstName,
                LastName = request.LearnerUpdateDetails.LastName,
                EmailAddress = request.LearnerUpdateDetails.EmailAddress,
                DateOfBirth = request.LearnerUpdateDetails.DateOfBirth,
                Uln = request.LearnerUpdateDetails.Uln
            },
            LearningSupport = request.LearningSupport.SelectOrEmptyList(x =>
                new Domain.Models.Shared.LearningSupportDetails
                {
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }),
            OnProgramme = new Domain.Models.ShortCourses.OnProgramme
            {
                CourseCode = request.OnProgramme.CourseCode,
                EmployerId = request.OnProgramme.EmployerId,
                Ukprn = request.OnProgramme.Ukprn,
                StartDate = request.OnProgramme.StartDate,
                WithdrawalDate = request.OnProgramme.WithdrawalDate,
                CompletionDate = request.OnProgramme.CompletionDate,
                ExpectedEndDate = request.OnProgramme.ExpectedEndDate,
                Milestones = request.OnProgramme.Milestones ?? new List<Milestone>(),
                Price = request.OnProgramme.Price
            }
        };
    }
}