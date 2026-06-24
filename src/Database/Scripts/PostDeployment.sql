--/*
--Post-deployment script
--*/

-- FLP-1890 (delete on release)
-- Backfill ShortCourseLearning.TrainingCode from ShortCourseEpisode.TrainingCode.
IF COL_LENGTH('[dbo].[ShortCourseLearning]', 'TrainingCode') IS NOT NULL
BEGIN
	UPDATE [scl]
	SET [scl].[TrainingCode] = [sce].[TrainingCode]
	FROM [dbo].[ShortCourseLearning] AS [scl]
	INNER JOIN [dbo].[ShortCourseEpisode] AS [sce]
	ON [sce].[LearningKey] = [scl].[Key]
	WHERE [scl].[TrainingCode] = ''
END

-- FLP-1868 (delete on release)
-- Backfill ShortCourseEpisode.CompletionDate from ShortCourseLearning.CompletionDate.
IF COL_LENGTH('[dbo].[ShortCourseEpisode]', 'CompletionDate') IS NOT NULL
BEGIN
	UPDATE [sce]
	SET [sce].[CompletionDate] = [scl].[CompletionDate]
	FROM [dbo].[ShortCourseEpisode] AS [sce]
	INNER JOIN [dbo].[ShortCourseLearning] AS [scl]
	ON [scl].[Key] = [sce].[LearningKey]
END