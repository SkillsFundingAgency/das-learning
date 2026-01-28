CREATE TABLE [dbo].[ApprenticeshipLearning]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ApprovalsApprenticeshipId] BIGINT NOT NULL,
    [Uln] NVARCHAR(10) NOT NULL, 
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATETIME NOT NULL,
    [ApprenticeshipHashedId] NVARCHAR(100) NULL, 
    [CompletionDate] DATETIME NULL,
    [EmailAddress] NVARCHAR(320) NULL,
    [HasEHCP] BIT NOT NULL DEFAULT 0, 
    [IsCareLeaver] BIT NOT NULL DEFAULT 0, 
    [CareLeaverEmployerConsentGiven] BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ApprenticeshipLearning_ApprovalsApprenticeshipId_Uln UNIQUE (ApprovalsApprenticeshipId, Uln)
)
GO
CREATE INDEX IX_ApprenticeshipLearning_Uln ON ApprenticeshipLearning (Uln)
GO