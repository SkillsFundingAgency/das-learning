CREATE TABLE [dbo].[Episode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[LearningKey] UNIQUEIDENTIFIER NOT NULL, 
	[IsDeleted] BIT NOT NULL DEFAULT(0),
    [Ukprn] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [FundingPlatform] INT NULL,
    [FundingEmployerAccountId] BIGINT NULL, 
    [LegalEntityName] NVARCHAR(255) NOT NULL,
    [AccountLegalEntityId] BIGINT NULL,
    [TrainingCode] NCHAR(10) NOT NULL,
    [TrainingCourseVersion] NVARCHAR(10) NULL,
    [PaymentsFrozen] BIT NOT NULL DEFAULT (0), 
    [LastDayOfLearning] DATETIME NULL, 
    [PauseDate] DATETIME NULL,
    [FundingBandMaximum] INT NULL
)
GO
ALTER TABLE dbo.Episode
ADD CONSTRAINT FK_Episode_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.Learning ([Key])
GO
CREATE INDEX IX_LearningKey ON [dbo].[Episode] (LearningKey);
GO
CREATE NONCLUSTERED INDEX [IX_Ukprn] ON [dbo].[Episode]
(
	[Ukprn] ASC
)
GO
