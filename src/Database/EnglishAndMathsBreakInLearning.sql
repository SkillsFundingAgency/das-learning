CREATE TABLE [dbo].[EnglishAndMathsBreakInLearning]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [PriorPeriodExpectedEndDate] DATETIME NOT NULL
);
GO

ALTER TABLE dbo.EnglishAndMathsBreakInLearning
ADD CONSTRAINT FK_EnglishAndMathsBreakInLearning_EnglishAndMaths
    FOREIGN KEY (EnglishAndMathsKey) REFERENCES dbo.EnglishAndMaths ([Key]);
GO
