-- Script to clear all data from the database

BEGIN TRAN

DELETE FROM [dbo].[MathsAndEnglishBreakInLearning];
DELETE FROM [dbo].[EpisodeBreakInLearning];
DELETE FROM [dbo].[EpisodePrice];
DELETE FROM [dbo].[LearningSupport];

DELETE FROM [dbo].[MathsAndEnglish];
DELETE FROM [dbo].[Episode];

DELETE FROM [dbo].[FreezeRequest];
DELETE FROM [History].[LearningHistory];

DELETE FROM [dbo].[Learning];

ROLLBACK TRAN
--COMMIT TRAN

GO
