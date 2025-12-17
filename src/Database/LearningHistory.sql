CREATE SCHEMA [History]
GO

CREATE TABLE [History].[LearningHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
    [State] NVARCHAR(MAX) NOT NULL
)
GO

ALTER TABLE [History].[LearningHistory]
ADD CONSTRAINT FK_LearningHistory_Learning FOREIGN KEY ([LearningId])
    REFERENCES [dbo].[Learning] ([Key])
GO

CREATE NONCLUSTERED INDEX [IX_LearningHistory_LearningId]
    ON [History].[LearningHistory] ([LearningId] ASC)
GO
