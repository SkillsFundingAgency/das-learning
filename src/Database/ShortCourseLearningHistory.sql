CREATE TABLE [History].[ShortCourseLearningHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] INT NULL,
    [Operation] NVARCHAR(20) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [State] NVARCHAR(MAX) NOT NULL
)
GO

ALTER TABLE [History].[ShortCourseLearningHistory]
ADD CONSTRAINT FK_ShortCourseLearningHistory_ShortCourseLearning FOREIGN KEY ([LearningKey])
    REFERENCES [dbo].[ShortCourseLearning] ([Key])
GO

CREATE NONCLUSTERED INDEX [IX_ShortCourseLearningHistory_LearningKey]
    ON [History].[ShortCourseLearningHistory] ([LearningKey] ASC)
GO
