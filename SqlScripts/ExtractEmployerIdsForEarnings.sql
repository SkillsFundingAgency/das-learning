SELECT
	'BEGIN TRANSACTION' As Script

UNION ALL

SELECT
	'
		UPDATE Domain.ShortCourseEpisode
		SET EmployerAccountId = ' + CONVERT(nvarchar(200), Lrn.EmployerAccountId) + ',
		FundingEmployerAccountId = ' + CONVERT(nvarchar(200), Lrn.FundingEmployerAccountId) + '
		WHERE [Key] = ''' + CONVERT(nvarchar(200), Lrn.EpisodeKey) + ''' AND EmployerAccountId IS NUll AND FundingEmployerAccountId IS NULL
	'
FROM
	(
		SELECT 
			[Key] As EpisodeKey, 
			EmployerAccountId, 
			CASE WHEN TransferSenderId IS NULL THEN EmployerAccountId ELSE TransferSenderId END As FundingEmployerAccountId
		FROM 
			ShortCourseEpisode 
		WHERE 
			IsApproved = 1
	) As Lrn

UNION ALL

SELECT '--COMMIT;'
UNION ALL
SELECT 'ROLLBACK;'