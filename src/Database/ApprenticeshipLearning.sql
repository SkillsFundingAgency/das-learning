CREATE TABLE [dbo].[ApprenticeshipLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearnerKey] UNIQUEIDENTIFIER NOT NULL,
    [ApprovalsApprenticeshipId] BIGINT NOT NULL,
    [CompletionDate] DATETIME NULL,
    CONSTRAINT UQ_ApprenticeshipLearning_ApprovalsApprenticeshipId UNIQUE (ApprovalsApprenticeshipId)
)
    GO
CREATE INDEX IX_ApprenticeshipLearning_LearnerKey ON ApprenticeshipLearning (LearnerKey)
    GO

GO
ALTER TABLE dbo.ApprenticeshipLearning
    ADD CONSTRAINT FK_ApprenticeshipLearning_Learner FOREIGN KEY (LearnerKey) REFERENCES dbo.Learner ([Key])
    GO
