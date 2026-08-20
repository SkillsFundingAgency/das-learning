using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SFA.DAS.Learning.Command.UnitTests;

internal static class TestHelper
{
    internal static void SetEpisode(ApprenticeshipLearningDomainModel learning, ApprenticeshipEpisodeDomainModel episode)
    {
        // Use reflection to set the private _episodes field so that there is only one
        typeof(ApprenticeshipLearningDomainModel)
            .GetField("_episodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(learning, new List<ApprenticeshipEpisodeDomainModel> { episode });
    }

    internal static void SetCompletionDate(ApprenticeshipLearningDomainModel learning, DateTime? completionDate)
    {
        // Use reflection to control CompletionDate deterministically for academic-year-overlap assertions
        var entityField = typeof(LearningDomainModel<ApprenticeshipLearning>)
            .GetField("_entity", BindingFlags.Instance | BindingFlags.NonPublic);
        var entity = (ApprenticeshipLearning)entityField!.GetValue(learning)!;
        entity.CompletionDate = completionDate;
    }
}
