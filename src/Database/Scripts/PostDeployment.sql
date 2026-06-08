--/*
--Post-deployment script
--*/

-- FLP-1868 (delete this script once CompletionDate is dropped from ShortCourseLearning)
-- Backfill ShortCourseEpisode.CompletionDate from ShortCourseLearning.CompletionDate.
IF COL_LENGTH('[dbo].[ShortCourseEpisode]', 'CompletionDate') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseLearning]', 'CompletionDate') IS NOT NULL
BEGIN
    UPDATE [sce]
    SET [sce].[CompletionDate] = [scl].[CompletionDate]
    FROM [dbo].[ShortCourseEpisode] AS [sce]
    INNER JOIN [dbo].[ShortCourseLearning] AS [scl]
        ON [scl].[Key] = [sce].[LearningKey]
    WHERE [scl].[CompletionDate] IS NOT NULL
      AND [sce].[IsRemoved] = 0
      AND [sce].[CompletionDate] IS NULL
END