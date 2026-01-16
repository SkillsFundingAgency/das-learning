CREATE TABLE [dbo].[MathsAndEnglishBreakInLearning]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[MathsAndEnglishKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL, 
    [PriorPeriodExpectedEndDate] DATETIME NOT NULL
);
GO

ALTER TABLE dbo.MathsAndEnglishBreakInLearning
ADD CONSTRAINT FK_MathsAndEnglishBreakInLearning_MathsAndEnglish
    FOREIGN KEY (MathsAndEnglishKey) REFERENCES dbo.MathsAndEnglish ([Key]);
GO