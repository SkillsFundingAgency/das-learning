CREATE TABLE [dbo].[EnglishAndMaths]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [PlannedEndDate] DATETIME NOT NULL,
    [Course] NCHAR(50) NOT NULL,
    [WithdrawalDate] DATETIME NULL,
    [CompletionDate] DATETIME NULL,
    [Amount] DECIMAL(15, 5) NOT NULL,
    [PauseDate] DATETIME NULL,
    [LearnAimRef] VARCHAR(8) NOT NULL DEFAULT '',
    [CombinedFundingAdjustmentPercentage] DECIMAL(18, 6) NULL
)
GO

ALTER TABLE [dbo].[EnglishAndMaths]
ADD CONSTRAINT FK_EnglishAndMaths_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ApprenticeshipLearning ([Key])
GO

CREATE INDEX IX_LearningKey ON [dbo].[EnglishAndMaths] (LearningKey);
GO
