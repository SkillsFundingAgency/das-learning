using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class SqlConnectionExtensions
{
    internal static DataAccess.Entities.Learning.Learner GetLearner(this SqlConnection dbConnection, string uln)
    {
        var learner = dbConnection.GetAll<DataAccess.Entities.Learning.Learner>().Single(x => x.Uln == uln);

        return learner;
    }

    internal static DataAccess.Entities.Learning.ApprenticeshipLearning GetLearningByLearnerKey(this SqlConnection dbConnection, Guid learnerKey)
    {
        var learning = dbConnection.GetAll<DataAccess.Entities.Learning.ApprenticeshipLearning>().Single(x => x.LearnerKey == learnerKey);
        learning.Episodes = dbConnection.GetAll<DataAccess.Entities.Learning.ApprenticeshipEpisode>().Where(x => x.LearningKey == learning.Key).ToList();
        learning.MathsAndEnglishCourses = dbConnection.GetAll<DataAccess.Entities.Learning.MathsAndEnglish>().Where(x => x.LearningKey == learning.Key).ToList();

        foreach (var episode in learning.Episodes)
        {
            episode.Prices = dbConnection.GetAll<DataAccess.Entities.Learning.EpisodePrice>().Where(x => x.EpisodeKey == episode.Key).ToList();
            episode.LearningSupport = dbConnection.GetAll<DataAccess.Entities.Learning.ApprenticeshipLearningSupport>().Where(x => x.EpisodeKey == episode.Key).ToList();
            episode.BreaksInLearning = dbConnection.GetAll<DataAccess.Entities.Learning.EpisodeBreakInLearning>().Where(x => x.EpisodeKey == episode.Key).ToList();
        }

        foreach (var mathsAndEnglish in learning.MathsAndEnglishCourses)
        {
            mathsAndEnglish.BreaksInLearning = dbConnection.GetAll<DataAccess.Entities.Learning.MathsAndEnglishBreakInLearning>().Where(x => x.MathsAndEnglishKey == mathsAndEnglish.Key).ToList();
        }

        return learning;
    }

    internal static DataAccess.Entities.Learning.ApprenticeshipLearning GetLearning(this SqlConnection dbConnection, string uln)
    {
        var learner = dbConnection.GetLearner(uln);
        var learning = dbConnection.GetLearningByLearnerKey(learner.Key);
        return learning;
    }

    internal static List<DataAccess.Entities.Learning.LearningHistory> GetHistories(this SqlConnection dbConnection, Guid learningKey)
    {
        return dbConnection.GetAll<DataAccess.Entities.Learning.LearningHistory>().Where(x => x.LearningId == learningKey).ToList();
    }

    internal static Guid GetLearningKey(this SqlConnection dbConnection, string uln)
    {
        var learner = dbConnection.GetLearner(uln);
        var learning = dbConnection.GetAll<DataAccess.Entities.Learning.ApprenticeshipLearning>().Single(x => x.LearnerKey == learner.Key);
        return learning.Key;
    }

    internal static DataAccess.Entities.Learning.Learner GetLearnerByShortCourseKey(this SqlConnection dbConnection, Guid shortCourseLearningKey)
    {
        var shortCourse = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseLearning>().Single(x => x.Key == shortCourseLearningKey);
        var learner = dbConnection.GetAll<DataAccess.Entities.Learning.Learner>().Single(x => x.Key == shortCourse.LearnerKey);
        return learner;
    }

    internal static DataAccess.Entities.Learning.ShortCourseLearning GetShortCourseLearning(this SqlConnection dbConnection, Guid shortCourseLearningKey)
    {
        var shortCourse = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseLearning>().Single(x => x.Key == shortCourseLearningKey);
        shortCourse.Episodes = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseEpisode>().Where(x => x.LearningKey == shortCourse.Key).ToList();
        
        foreach (var episode in shortCourse.Episodes)
        {
            episode.Milestones = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseMilestone>().Where(x => x.EpisodeKey == episode.Key).ToList();
            episode.LearningSupport = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseLearningSupport>().Where(x => x.EpisodeKey == episode.Key).ToList();
        }

        return shortCourse;
    }

    internal static List<DataAccess.Entities.Learning.ShortCourseLearning> GetShortCourseLearningsForLearner(this SqlConnection dbConnection, Guid learnerKey)
    {
        var shortCourses = dbConnection.GetAll<DataAccess.Entities.Learning.ShortCourseLearning>()
            .Where(x => x.LearnerKey == learnerKey);

        return shortCourses.ToList();
    }
}
