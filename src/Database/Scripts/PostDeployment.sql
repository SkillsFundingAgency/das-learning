--/*
--Post-deployment script
--*/

-- FLP-2068 Backfill TrainingCode/TrainingCourseVersion from episode to learning. (todo delete this and drop legacy columns [TrainingCode], [TrainingCourseVersion] from ApprenticeshipEpisode once all environments have migrated data to ApprenticeshipLearning)
-- Assumes 1:1 Apprenticeship -> Episode relationship
IF COL_LENGTH('[dbo].[ApprenticeshipLearning]', 'TrainingCode') IS NOT NULL
   AND COL_LENGTH('[dbo].[ApprenticeshipEpisode]', 'TrainingCode') IS NOT NULL
BEGIN
	UPDATE [al]
	SET [al].[TrainingCode] = [ae].[TrainingCode],
		[al].[TrainingCourseVersion] = [ae].[TrainingCourseVersion]
	FROM [dbo].[ApprenticeshipLearning] AS [al]
	INNER JOIN [dbo].[ApprenticeshipEpisode] AS [ae]
	ON [ae].[LearningKey] = [al].[Key]
	WHERE [al].[TrainingCode] = ''
END

-- FLP-2068 Backfill CompletionDate/AchievementDate from learning to episode. (todo delete this and drop legacy columns [CompletionDate], [AchievementDate] from ApprenticeshipLearning once all environments have migrated data to ApprenticeshipEpisode)
-- Assumes 1:1 Apprenticeship -> Episode relationship
IF COL_LENGTH('[dbo].[ApprenticeshipEpisode]', 'CompletionDate') IS NOT NULL
   AND COL_LENGTH('[dbo].[ApprenticeshipLearning]', 'CompletionDate') IS NOT NULL
BEGIN
	UPDATE [ae]
	SET [ae].[CompletionDate] = [al].[CompletionDate],
		[ae].[AchievementDate] = [al].[AchievementDate]
	FROM [dbo].[ApprenticeshipEpisode] AS [ae]
	INNER JOIN [dbo].[ApprenticeshipLearning] AS [al]
	ON [al].[Key] = [ae].[LearningKey]
	WHERE [ae].[CompletionDate] IS NULL AND [al].[CompletionDate] IS NOT NULL
	   OR [ae].[AchievementDate] IS NULL AND [al].[AchievementDate] IS NOT NULL
END