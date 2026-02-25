/*
Pre-deployment script
*/

BEGIN TRAN

    DELETE FROM [dbo].[MathsAndEnglishBreakInLearning];
    DELETE FROM [dbo].[EpisodeBreakInLearning];
    DELETE FROM [dbo].[EpisodePrice];

    DELETE FROM [dbo].[MathsAndEnglish];
    DELETE FROM [dbo].[Episode];

    DELETE FROM [History].[LearningHistory];

    DELETE FROM [dbo].[Learning];

COMMIT TRAN