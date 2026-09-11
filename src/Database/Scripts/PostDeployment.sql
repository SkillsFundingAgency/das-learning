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