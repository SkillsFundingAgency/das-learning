/*
Pre-deployment script
*/

IF EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'Episode' -- check that one of the tables removed in FLP-1493 exists in the db
      AND s.name = 'dbo'
)
BEGIN
    BEGIN TRAN

    DELETE FROM [dbo].[MathsAndEnglishBreakInLearning];
    DELETE FROM [dbo].[EpisodeBreakInLearning];
    DELETE FROM [dbo].[EpisodePrice];
    DELETE FROM [dbo].[LearningSupport];

    DELETE FROM [dbo].[MathsAndEnglish];
    DELETE FROM [dbo].[Episode];

	DELETE FROM [dbo].[FreezeRequest]
    DELETE FROM [History].[LearningHistory];

    DELETE FROM [dbo].[Learning];

    COMMIT TRAN
END
GO