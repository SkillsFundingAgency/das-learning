CREATE TABLE [dbo].[MathsAndEnglish]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [PlannedEndDate] DATETIME NOT NULL,
    [Course] NCHAR(50) NOT NULL,
    [WithdrawalDate] DATETIME NULL,
    [PriorLearningPercentage] INT NULL
)
GO

ALTER TABLE [dbo].[MathsAndEnglish]
ADD CONSTRAINT FK_MathsAndEnglish_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.Learning ([Key])
GO

CREATE INDEX IX_LearningKey ON [dbo].[MathsAndEnglish] (LearningKey);
GO