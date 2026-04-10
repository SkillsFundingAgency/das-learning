--/*
--Post-deployment script
--*/

-- Set ApprenticeshipEpisode.ApprovalsApprenticeshipId from ApprenticeshipLearning
UPDATE ae
SET ae.ApprovalsApprenticeshipId = al.ApprovalsApprenticeshipId
FROM dbo.ApprenticeshipEpisode ae
INNER JOIN dbo.ApprenticeshipLearning al ON al.[Key] = ae.LearningKey
WHERE ae.ApprovalsApprenticeshipId = 0
GO
