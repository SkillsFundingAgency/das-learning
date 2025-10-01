using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class SqlConnectionExtensions
{
    internal static DataAccess.Entities.Learning.Learning GetLearning(this SqlConnection dbConnection, string uln)
    {
        var learning = dbConnection.GetAll<DataAccess.Entities.Learning.Learning>().Single(x => x.Uln == uln);
        learning.Episodes = dbConnection.GetAll<DataAccess.Entities.Learning.Episode>().Where(x => x.LearningKey == learning.Key).ToList();

        foreach (var episode in learning.Episodes)
        {
            episode.Prices = dbConnection.GetAll<DataAccess.Entities.Learning.EpisodePrice>().Where(x => x.EpisodeKey == episode.Key).ToList();
            episode.LearningSupport = dbConnection.GetAll<DataAccess.Entities.Learning.LearningSupport>().Where(x => x.EpisodeKey == episode.Key).ToList();
        }

        return learning;
    }

    internal static Guid GetLearningKey(this SqlConnection dbConnection, string uln)
    {
        var learning = dbConnection.GetAll<DataAccess.Entities.Learning.Learning>().Single(x => x.Uln == uln);
        return learning.Key;
    }
}
