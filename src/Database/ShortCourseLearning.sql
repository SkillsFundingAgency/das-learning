CREATE TABLE [dbo].[ShortCourseLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearnerKey] UNIQUEIDENTIFIER NOT NULL,
    [CompletionDate] DATETIME NULL
)
GO

CREATE INDEX IX_ShortCourseLearning_LearnerKey ON ShortCourseLearning (LearnerKey)
GO
ALTER TABLE dbo.ShortCourseLearning
ADD CONSTRAINT FK_ShortCourseLearning_Learner FOREIGN KEY (LearnerKey) REFERENCES dbo.Learner ([Key])
GO