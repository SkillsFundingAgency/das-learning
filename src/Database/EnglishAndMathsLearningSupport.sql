CREATE TABLE [dbo].[EnglishAndMathsLearningSupport]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL
);
GO

ALTER TABLE dbo.EnglishAndMathsLearningSupport
ADD CONSTRAINT FK_EnglishAndMathsLearningSupport_EnglishAndMaths
    FOREIGN KEY (EnglishAndMathsKey) REFERENCES dbo.EnglishAndMaths ([Key]);
GO

CREATE NONCLUSTERED INDEX IX_EnglishAndMathsLearningSupport_EnglishAndMathsKey
    ON [dbo].[EnglishAndMathsLearningSupport] ([EnglishAndMathsKey]);
GO
