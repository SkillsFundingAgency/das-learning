SELECT
	'BEGIN TRANSACTION' As Script

UNION ALL

SELECT
	'
		UPDATE Domain.ShortCourseLearning
		SET ApprovalsApprenticeshipId = ' + CONVERT(nvarchar(200), Lrn.ApprovalsApprenticeshipId) + '
		WHERE [LearningKey] = ''' + CONVERT(nvarchar(200), Lrn.LearningKey) + ''' AND ApprovalsApprenticeshipId = 0
	'
FROM
	(
		SELECT DISTINCT
			[LearningKey] As LearningKey, 
			ApprovalsApprenticeshipId
		FROM 
			ShortCourseEpisode 
		WHERE 
			IsApproved = 1
	) As Lrn

UNION ALL

SELECT '--COMMIT;'
UNION ALL
SELECT 'ROLLBACK;'