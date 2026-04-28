/*
Pre-deployment script
*/

IF OBJECT_ID(N'[dbo].[Approval]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[Approval];
    END
GO

IF OBJECT_ID(N'[dbo].[ClientOutboxData]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[ClientOutboxData];
    END
GO

IF OBJECT_ID(N'[dbo].[Episode]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[Episode];
    END
GO

IF OBJECT_ID(N'[dbo].[FreezeRequest]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[FreezeRequest];
    END
GO

IF OBJECT_ID(N'[dbo].[Learning]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[Learning];
    END
GO

IF OBJECT_ID(N'[dbo].[LearningSupport]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[LearningSupport];
    END
GO

IF OBJECT_ID(N'[dbo].[MathsAndEnglish]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[MathsAndEnglish];
    END
GO

IF OBJECT_ID(N'[dbo].[MathsAndEnglishBreakInLearning]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[MathsAndEnglishBreakInLearning];
    END
GO

IF OBJECT_ID(N'[dbo].[OutboxData]', N'U') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[OutboxData];
    END
GO