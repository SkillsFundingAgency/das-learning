using System.Linq;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.DataAccess.Extensions;

namespace SFA.DAS.Learning.Domain.UnitTests.Helpers;

public static class LearnerUpdateModelHelper
{
    public static LearnerUpdateModel CreateFromLearningEntity(DataAccess.Entities.Learning.Learning learning)
    {
        return new LearnerUpdateModel
        {
            Learning = new LearningUpdateDetails
            {
                CompletionDate = learning.CompletionDate
            },
            MathsAndEnglishCourses = learning.MathsAndEnglishCourses.Select(x => new MathsAndEnglishUpdateDetails
            {
                Course = x.Course,
                StartDate = x.StartDate,
                PlannedEndDate = x.PlannedEndDate,
                CompletionDate = x.CompletionDate,
                WithdrawalDate = x.WithdrawalDate,
                PriorLearningPercentage = x.PriorLearningPercentage,
                Amount = x.Amount
            }).ToList(),
            LearningSupport = learning.GetEpisode().LearningSupport.Select(x => new LearningSupportDetails
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToList()
        };
    }
}