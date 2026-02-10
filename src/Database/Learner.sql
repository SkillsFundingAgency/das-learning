CREATE TABLE [dbo].[Learner]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Uln] NVARCHAR(10) NOT NULL, 
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [DateOfBirth] DATETIME NOT NULL,
    [EmailAddress] NVARCHAR(320) NULL,
    [HasEHCP] BIT NOT NULL DEFAULT 0, 
    [IsCareLeaver] BIT NOT NULL DEFAULT 0, 
    [CareLeaverEmployerConsentGiven] BIT NOT NULL DEFAULT 0
)
GO
CREATE INDEX IX_Learner_Uln ON Learner (Uln)
GO