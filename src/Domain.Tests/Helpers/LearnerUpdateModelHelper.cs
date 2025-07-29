using System.Linq;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Domain.UnitTests.Helpers;

public static class LearnerUpdateModelHelper
{
    public static LearnerUpdateModel CreateFromLearningEntity(DataAccess.Entities.Learning.Learning learning)
    {
        return new LearnerUpdateModel(
            new LearningUpdateDetails(learning.CompletionDate),
            learning.MathsAndEnglishCourses.Select(x => new MathsAndEnglishUpdateDetails(x.Course, x.StartDate, x.PlannedEndDate, x.CompletionDate, x.WithdrawalDate, x.PriorLearningPercentage, x.Amount)).ToList());
    }
}