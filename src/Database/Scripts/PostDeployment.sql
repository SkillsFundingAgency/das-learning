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

-- FLP-1898 (todo delete this and drop the legacy columns [Price], [LearningType] from ShortCourseEpisode once all environments have migrated data to ShortCourseLearning)
IF COL_LENGTH('[dbo].[ShortCourseLearning]', 'Price') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseLearning]', 'LearningType') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseEpisode]', 'Price') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseEpisode]', 'LearningType') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseEpisode]', 'StartDate') IS NOT NULL
BEGIN
	;WITH [LatestEpisodePerLearning] AS
	(
		SELECT
			[sce].[LearningKey],
			[sce].[Price],
			[sce].[LearningType],
			ROW_NUMBER() OVER (
				PARTITION BY [sce].[LearningKey]
				ORDER BY [sce].[StartDate] DESC, [sce].[Key] DESC
			) AS [RowNumber]
		FROM [dbo].[ShortCourseEpisode] AS [sce]
	)
	UPDATE [scl]
	SET [scl].[Price] = [le].[Price],
		[scl].[LearningType] = [le].[LearningType]
	FROM [dbo].[ShortCourseLearning] AS [scl]
	INNER JOIN [LatestEpisodePerLearning] AS [le]
	ON [le].[LearningKey] = [scl].[Key]
	AND [le].[RowNumber] = 1
	WHERE [scl].[Price] = 0
END