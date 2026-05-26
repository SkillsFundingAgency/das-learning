CREATE TABLE [dbo].[ApprenticeshipEpisode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [ApprovalsApprenticeshipId] BIGINT NOT NULL DEFAULT (0),
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
    [WithdrawalDate] DATETIME NULL, 
    [PauseDate] DATETIME NULL
)
GO
ALTER TABLE dbo.ApprenticeshipEpisode
ADD CONSTRAINT FK_ApprenticeshipEpisode_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ApprenticeshipLearning ([Key])
GO
CREATE INDEX IX_LearningKey ON [dbo].[ApprenticeshipEpisode] (LearningKey);
GO
CREATE NONCLUSTERED INDEX [IX_Ukprn] ON [dbo].[ApprenticeshipEpisode]
(
	[Ukprn] ASC
)
GO
