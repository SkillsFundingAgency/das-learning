using AutoFixture;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;

namespace SFA.DAS.Learning.Command.UnitTests;

internal static class AutoFixtureExtensions
{
    internal static EpisodeDomainModel CreateEpisodeDomainModel(this Fixture fixture, Action<Episode>? configure = null)
    {
        var entityModel = fixture.Create<Episode>();

        configure?.Invoke(entityModel); // apply custom changes

        var domainModel = EpisodeDomainModel.Get(entityModel);

        return domainModel;
    }
}
