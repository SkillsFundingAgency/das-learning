CREATE TABLE [History].[ApprenticeshipLearningHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] INT NULL,
    [Operation] NVARCHAR(20) NOT NULL,
    [Changes] NVARCHAR(200) NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [State] NVARCHAR(MAX) NOT NULL
)
GO

ALTER TABLE [History].[ApprenticeshipLearningHistory]
ADD CONSTRAINT FK_ApprenticeshipLearningHistory_ApprenticeshipLearning FOREIGN KEY ([LearningKey])
    REFERENCES [dbo].[ApprenticeshipLearning] ([Key])
GO

CREATE NONCLUSTERED INDEX [IX_ApprenticeshipLearningHistory_LearningKey]
    ON [History].[ApprenticeshipLearningHistory] ([LearningKey] ASC)
GO
