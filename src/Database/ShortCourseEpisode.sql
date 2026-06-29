CREATE TABLE [dbo].[ShortCourseEpisode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [Ukprn] BIGINT NOT NULL,
    [EmployerAccountId] BIGINT NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [ExpectedEndDate] DATETIME NOT NULL,
    [WithdrawalDate] DATETIME NULL,
    [WithdrawalReason] SMALLINT NULL,
    [TrainingCode] VARCHAR(8) NOT NULL DEFAULT '',
    [ApprovalsApprenticeshipId] BIGINT NOT NULL DEFAULT 0,
    [IsApproved] BIT NOT NULL DEFAULT 0,
    [Price] MONEY NOT NULL DEFAULT 0,
    [LearnerRef] VARCHAR(128) NOT NULL DEFAULT '',
    [LearningType] TINYINT NOT NULL DEFAULT 0,
    [EmployerType] TINYINT NOT NULL DEFAULT 0,
    [TransferSenderId] BIGINT NULL,
    [IsRemoved] BIT NOT NULL DEFAULT 0,
    [CompletionDate] DATETIME NULL
)
GO

ALTER TABLE dbo.ShortCourseEpisode
ADD CONSTRAINT FK_ShortCourseEpisode_ShortCourseLearning
    FOREIGN KEY (LearningKey)
    REFERENCES dbo.ShortCourseLearning ([Key])
GO

CREATE INDEX IX_ShortCourseEpisode_LearningKey
    ON [dbo].[ShortCourseEpisode] ([LearningKey])
GO

CREATE NONCLUSTERED INDEX IX_ShortCourseEpisode_Ukprn
    ON [dbo].[ShortCourseEpisode] ([Ukprn])
GO

CREATE NONCLUSTERED INDEX IX_ShortCourseEpisode_Ukprn_Approval
    ON [dbo].[ShortCourseEpisode] ([Ukprn], [IsApproved], [IsRemoved], [StartDate])
    INCLUDE ([LearningKey], [WithdrawalDate], [CompletionDate])
GO
