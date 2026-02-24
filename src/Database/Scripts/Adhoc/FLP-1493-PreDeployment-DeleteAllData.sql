-- Disable referential integrity to avoid foreign key constraint errors
ALTER TABLE [History].[LearningHistory] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[MathsAndEnglishBreakInLearning] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[MathsAndEnglish] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[EpisodeBreakInLearning] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[EpisodePrice] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Episode] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[LearningSupport] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[FreezeRequest] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Learning] NOCHECK CONSTRAINT ALL;
GO

-- Delete all data in the tables
DELETE FROM [History].[LearningHistory];
DELETE FROM [dbo].[MathsAndEnglishBreakInLearning];
DELETE FROM [dbo].[MathsAndEnglish];
DELETE FROM [dbo].[EpisodeBreakInLearning];
DELETE FROM [dbo].[EpisodePrice];
DELETE FROM [dbo].[Episode];
DELETE FROM [dbo].[LearningSupport];
DELETE FROM [dbo].[FreezeRequest];
DELETE FROM [dbo].[Learning];
GO

-- Re-enable referential integrity
ALTER TABLE [History].[LearningHistory] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[MathsAndEnglishBreakInLearning] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[MathsAndEnglish] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[EpisodeBreakInLearning] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[EpisodePrice] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Episode] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[LearningSupport] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[FreezeRequest] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Learning] WITH CHECK CHECK CONSTRAINT ALL;
GO
