--/*
--Post-deployment script
--*/


-- FLP-1898 (todo delete this and drop the legacy columns [Price], [LearningType] from ShortCourseEpisode once all environments have migrated data to ShortCourseLearning)
IF COL_LENGTH('[dbo].[ShortCourseLearning]', 'Price') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseLearning]', 'LearningType') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseEpisode]', 'Price') IS NOT NULL
   AND COL_LENGTH('[dbo].[ShortCourseEpisode]', 'LearningType') IS NOT NULL
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

-- FLP-1938 Backfill EmployerType from FundingType for existing apprenticeship episode rows. (todo delete this and drop legacy column [FundingType] from ApprenticeshipEpisode once all environments have migrated data to EmployerType)
IF COL_LENGTH('[dbo].[ApprenticeshipEpisode]', 'FundingType') IS NOT NULL
   AND COL_LENGTH('[dbo].[ApprenticeshipEpisode]', 'EmployerType') IS NOT NULL
BEGIN
	UPDATE [dbo].[ApprenticeshipEpisode]
	SET [EmployerType] = [FundingType]
	WHERE [EmployerType] IS NULL AND [FundingType] IS NOT NULL
END