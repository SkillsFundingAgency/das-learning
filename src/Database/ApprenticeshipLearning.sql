CREATE TABLE [dbo].[ApprenticeshipLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearnerKey] UNIQUEIDENTIFIER NOT NULL,
    [CompletionDate] DATETIME NULL,
    [AchievementDate] DATETIME NULL,
    [LearningType] TINYINT NOT NULL DEFAULT 0,
    [TrainingCode] NCHAR(10) NOT NULL DEFAULT '',
    [TrainingCourseVersion] NVARCHAR(10) NULL
)
    GO
CREATE INDEX IX_ApprenticeshipLearning_LearnerKey ON ApprenticeshipLearning (LearnerKey)
    GO

GO
ALTER TABLE dbo.ApprenticeshipLearning
    ADD CONSTRAINT FK_ApprenticeshipLearning_Learner FOREIGN KEY (LearnerKey) REFERENCES dbo.Learner ([Key])
    GO
