using SFA.DAS.Learning.Domain.Apprenticeship;
using System.Collections.Generic;
using System.Reflection;

namespace SFA.DAS.Learning.Command.UnitTests;

internal static class TestHelper
{
    internal static void SetEpisode(LearningDomainModel learning, EpisodeDomainModel episode)
    {
        // Use reflection to set the private _episodes field so that there is only one
        typeof(LearningDomainModel)
            .GetField("_episodes", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(learning, new List<EpisodeDomainModel> { episode });
    }
}
