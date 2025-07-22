CREATE TABLE [dbo].[Learning]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ApprovalsApprenticeshipId] BIGINT NOT NULL,
    [Uln] NVARCHAR(10) NOT NULL, 
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATETIME NOT NULL,
    [ApprenticeshipHashedId] NVARCHAR(100) NULL, 
    [CompletionDate] DATETIME NULL,
    CONSTRAINT UQ_Learning_ApprovalsApprenticeshipId_Uln UNIQUE (ApprovalsApprenticeshipId, Uln)
)
GO
CREATE INDEX IX_Learning_Uln ON Learning (Uln)
GO