CREATE TABLE [dbo].[FreezeRequest]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [LearningKey] UNIQUEIDENTIFIER NOT NULL, 
    [FrozenBy] NVARCHAR(500) NOT NULL, 
    [FrozenDateTime] DATETIME NOT NULL, 
    [Unfrozen] BIT NOT NULL, 
    [UnfrozenDateTime] DATETIME NULL, 
    [UnfrozenBy] NVARCHAR(500) NULL, 
    [Reason] NVARCHAR(200) NULL 
)

GO
ALTER TABLE [dbo].[FreezeRequest]
ADD CONSTRAINT FK_FreezeRequest_Learning FOREIGN KEY (LearningKey) REFERENCES dbo.ApprenticeshipLearning ([Key])
GO
CREATE INDEX IX_LearningKey ON [dbo].[FreezeRequest] (LearningKey);
GO