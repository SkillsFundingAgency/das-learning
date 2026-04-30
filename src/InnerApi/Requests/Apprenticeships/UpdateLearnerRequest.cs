using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.InnerApi.Requests.Shared;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using LearningSupportDetails = SFA.DAS.Learning.InnerApi.Requests.Shared.LearningSupportDetails;

namespace SFA.DAS.Learning.InnerApi.Requests.Apprenticeships;

#pragma warning disable CS8618 // Required properties must be set in the constructor

/// <summary>
/// Request to update a learner's details
/// </summary>
public class UpdateLearnerRequest
{
    ///<summary>
    /// Delivery details
    /// </summary>
    public Delivery Delivery { get; set; }

    /// <summary>
    /// Learner details to be updated
    /// </summary>
    public ApprenticeshipLearnerUpdateDetails Learner { get; set; }

    /// <summary>
    /// English and M course details
    /// </summary>
    public List<EnglishAndMaths> EnglishAndMathsCourses { get; set; }

    /// <summary>
    /// Learning support details
    /// </summary>
    public List<LearningSupportDetails> LearningSupport { get; set; }

    /// <summary>
    /// OnProgramme details
    /// </summary>
    public OnProgrammeDetails OnProgramme { get; set; }
}

/// <summary>
/// Delivery Details
/// </summary>
public class Delivery
{
    /// <summary>
    /// Withdrawal during learning date
    /// </summary>
    public DateTime? WithdrawalDate { get; set; }
}

/// <summary>
/// Learner details to be updated for Apprenticeships
/// </summary>
public class ApprenticeshipLearnerUpdateDetails : LearnerUpdateDetails
{
    /// <summary>
    /// Learner care details
    /// </summary>
    public CareDetails Care { get; set; }

    /// <summary>
    /// Date the learning completes, this will be null until completion is confirmed
    /// </summary>
    public DateTime? CompletionDate { get; set; }
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

/// <summary>
/// OnProgramme details
/// </summary>
public class OnProgrammeDetails
{
    /// <summary>
    /// Costs / Prices for the OnProgramme delivery
    /// </summary>
    public List<Cost> Costs { get; set; }

    /// <summary>
    /// Planned end date for the OnProgramme delivery
    /// </summary>
    public DateTime ExpectedEndDate { get; set; }

    /// <summary>
    /// Pause date for the OnProgramme delivery
    /// </summary>
    public DateTime? PauseDate { get; set; }

    /// <summary>
    /// Breaks in learning for the OnProgramme delivery
    /// </summary>
    public List<BreakInLearning> BreaksInLearning { get; set; }
}

/// <summary>
/// Cost details
/// </summary>
public class Cost
{
    /// <summary>
    /// The cost of the training, aka TNP1
    /// </summary>
    public int TrainingPrice { get; set; }

    /// <summary>
    /// The cost of the end-point assessment, aka TNP2
    /// </summary>
    public int? EpaoPrice { get; set; }
    
    /// <summary>
    /// The date from which this price applies
    /// </summary>
    public DateTime FromDate { get; set; }
}

/// <summary>
/// English and Maths course details
/// </summary>
public class EnglishAndMaths
{
    /// <summary>
    /// The standard code of the linked OnProg
    /// </summary>
    public string Course { get; set; }

    /// <summary>
    /// The english/maths course the learner is undertaking
    /// </summary>
    public string LearnAimRef { get; set; }

    /// <summary>
    /// Start date of the english or maths course
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Planned end date of the english or maths course
    /// </summary>
    public DateTime PlannedEndDate { get; set; }

    /// <summary>
    /// Date the english or maths course completes, this will be null until completion is confirmed
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Withdrawal date for the english or maths course, this will be null until a withdrawal is confirmed
    /// </summary>
    public DateTime? WithdrawalDate { get; set; }

    /// <summary>
    /// Pause date for this english or maths course
    /// </summary>
    public DateTime? PauseDate { get; set; }

    /// <summary>
    /// Combination of PriorLearningAdjustment and OtherFundingAdjustment
    /// </summary>
    public int? CombinedFundingAdjustmentPercentage { get; set; }

    /// <summary>
    /// Amount associated with the english or maths course
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Breaks in learning for the english or maths course
    /// </summary>
    public List<BreakInLearning> BreaksInLearning { get; set; }
}

/// <summary>
/// Break in learning details
/// </summary>
public class BreakInLearning
{
    /// <summary>
    /// Start date of the break in learning
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the break in learning
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Expected end date of the period in learning which this break truncates
    /// </summary>
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}
#pragma warning restore CS8618 // Required properties must be set in the constructor

/// <summary>
/// Learner care details
/// </summary>
public class CareDetails
{
    /// <summary> Indicates whether the learner has an Education, Health, and Care Plan (EHCP). /// </summary>
    public bool HasEHCP { get; set; }
    /// <summary> Indicates whether the learner is a care leaver </summary>
    public bool IsCareLeaver { get; set; }
    /// <summary> Indicates whether consent has been given to share information about the learner being a care leaver. </summary>
    public bool CareLeaverEmployerConsentGiven { get; set; }
}

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
        return new LearningUpdateContext
        {
            LearningKey = learningKey,
            Learner = new LearnerModel
            {
                FirstName = request.Learner.FirstName,
                LastName = request.Learner.LastName,
                EmailAddress = request.Learner.EmailAddress,
                DateOfBirth = request.Learner.DateOfBirth
            },
            Care = new Models.UpdateModels.Shared.CareDetails
            {
                HasEHCP = request.Learner.Care.HasEHCP,
                IsCareLeaver = request.Learner.Care.IsCareLeaver,
                CareLeaverEmployerConsentGiven = request.Learner.Care.CareLeaverEmployerConsentGiven
            },
            Delivery = new DeliveryDetails
            {
                WithdrawalDate = request.Delivery.WithdrawalDate
            },
            Learning = new LearningUpdateDetails
            {
                CompletionDate = request.Learner.CompletionDate
                
            },
            EnglishAndMathsCourses = request.EnglishAndMathsCourses.SelectOrEmptyList(x =>
                new EnglishAndMathsUpdateDetails
                {
                    Course = x.Course,
                    LearnAimRef = x.LearnAimRef,
                    StartDate = x.StartDate,
                    PlannedEndDate = x.PlannedEndDate,
                    CompletionDate = x.CompletionDate,
                    WithdrawalDate = x.WithdrawalDate,
                    PauseDate = x.PauseDate,
                    CombinedFundingAdjustmentPercentage = x.CombinedFundingAdjustmentPercentage,
                    Amount = x.Amount,
                    BreaksInLearning = x.BreaksInLearning.SelectOrEmptyList(b => 
                        new BreakInLearningUpdateDetails
                        {
                            StartDate = b.StartDate,
                            EndDate = b.EndDate,
                            PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
                        })
                }),
            LearningSupport = request.LearningSupport.SelectOrEmptyList(x =>
                new Models.UpdateModels.Shared.LearningSupportDetails
                {
                    StartDate = x.StartDate, EndDate = x.EndDate
                }),
            OnProgrammeDetails = new Models.UpdateModels.OnProgrammeDetails
            {
                ExpectedEndDate = request.OnProgramme.ExpectedEndDate,
                Costs = request.OnProgramme.Costs.SelectOrEmptyList(x => new Models.UpdateModels.Cost
                {
                    TrainingPrice = x.TrainingPrice,
                    EpaoPrice = x.EpaoPrice,
                    FromDate = x.FromDate
                }),
                PauseDate = request.OnProgramme.PauseDate,
                BreaksInLearning = request.OnProgramme.BreaksInLearning.SelectOrEmptyList(x => 
                    new Models.UpdateModels.BreakInLearningUpdateDetails
                    {
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        PriorPeriodExpectedEndDate = x.PriorPeriodExpectedEndDate
                    })
            }
        };
    }

}