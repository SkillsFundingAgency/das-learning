/*
Post-deployment script
*/


--FLP-1493 - Data Migration

--Populate Learner table
;WITH Deduped AS
          (
              SELECT
                  L.Uln,
                  L.FirstName,
                  L.LastName,
                  L.DateOfBirth,
                  L.EmailAddress,
                  L.HasEHCP,
                  L.IsCareLeaver,
                  L.CareLeaverEmployerConsentGiven,
                  ROW_NUMBER() OVER (PARTITION BY L.Uln ORDER BY L.Uln) AS rn
              FROM dbo.Learning AS L
          )
 INSERT INTO dbo.Learner
(
    [Key],
    [Uln],
    [FirstName],
    [LastName],
    [DateOfBirth],
    [EmailAddress],
    [HasEHCP],
    [IsCareLeaver],
    [CareLeaverEmployerConsentGiven]
)
SELECT
    NEWID(),
    Uln,
    FirstName,
    LastName,
    DateOfBirth,
    EmailAddress,
    HasEHCP,
    IsCareLeaver,
    CareLeaverEmployerConsentGiven
FROM Deduped d
WHERE d.rn = 1
  AND NOT EXISTS (
    SELECT 1
    FROM dbo.Learner lr
    WHERE lr.Uln = d.Uln
);

-- Migrate ApprenticeshipLearning rows from Learning
INSERT INTO dbo.ApprenticeshipLearning
(
[Key],
    [LearnerKey],
    [ApprovalsApprenticeshipId],
    [CompletionDate]
)
SELECT
    NEWID() AS [Key],
    LR.[Key] AS LearnerKey,
    L.ApprovalsApprenticeshipId,
    L.CompletionDate
FROM dbo.Learning AS L
    INNER JOIN dbo.Learner AS LR
ON LR.Uln = L.Uln
WHERE NOT EXISTS
    (
    SELECT 1
    FROM dbo.ApprenticeshipLearning AL
    WHERE AL.LearnerKey = LR.[Key]
  AND AL.ApprovalsApprenticeshipId = L.ApprovalsApprenticeshipId
    );


-- Insert ApprenticeshipEpisode rows migrated from Episode
INSERT INTO dbo.ApprenticeshipEpisode
(
[Key],
    [LearningKey],
    [IsDeleted],
    [Ukprn],
    [EmployerAccountId],
    [FundingType],
    [FundingPlatform],
    [FundingEmployerAccountId],
    [LegalEntityName],
    [AccountLegalEntityId],
    [TrainingCode],
    [TrainingCourseVersion],
    [PaymentsFrozen],
    [WithdrawalDate],
    [PauseDate]
)
SELECT
    E.[Key],                         -- reuse the existing Episode key
    AL.[Key] AS LearningKey,         -- new FK to ApprenticeshipLearning
    E.IsDeleted,
    E.Ukprn,
    E.EmployerAccountId,
    E.FundingType,
    E.FundingPlatform,
    E.FundingEmployerAccountId,
    E.LegalEntityName,
    E.AccountLegalEntityId,
    E.TrainingCode,
    E.TrainingCourseVersion,
    E.PaymentsFrozen,
    E.LastDayOfLearning AS WithdrawalDate,
    E.PauseDate
FROM dbo.Episode AS E
    INNER JOIN dbo.Learning AS L
ON L.[Key] = E.LearningKey
    INNER JOIN dbo.Learner AS LR
    ON LR.Uln = L.Uln
    INNER JOIN dbo.ApprenticeshipLearning AS AL
    ON AL.LearnerKey = LR.[Key]
    AND AL.ApprovalsApprenticeshipId = L.ApprovalsApprenticeshipId
WHERE NOT EXISTS
    (
    SELECT 1
    FROM dbo.ApprenticeshipEpisode AE
    WHERE AE.[Key] = E.[Key]
    );



-----------------------------------------------------------------
-- Move LearningHistory FK from Learning → ApprenticeshipLearning
-----------------------------------------------------------------

-- 1. Drop old FK if it exists
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_LearningHistory_Learning'
      AND parent_object_id = OBJECT_ID('[History].[LearningHistory]')
)
BEGIN
ALTER TABLE [History].[LearningHistory]
DROP CONSTRAINT FK_LearningHistory_Learning;
END
GO

-- 2. Drop old index if it exists
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_LearningHistory_LearningId'
      AND object_id = OBJECT_ID('[History].[LearningHistory]')
)
BEGIN
DROP INDEX [IX_LearningHistory_LearningId]
    ON [History].[LearningHistory];
END
GO

-- 3. Create new FK to ApprenticeshipLearning
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_LearningHistory_ApprenticeshipLearning'
      AND parent_object_id = OBJECT_ID('[History].[LearningHistory]')
)
BEGIN
ALTER TABLE [History].[LearningHistory]
    ADD CONSTRAINT FK_LearningHistory_ApprenticeshipLearning
    FOREIGN KEY ([LearningId])
    REFERENCES [dbo].[ApprenticeshipLearning]([Key]);
END
GO

-- 4. Recreate the index
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_LearningHistory_LearningId'
      AND object_id = OBJECT_ID('[History].[LearningHistory]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_LearningHistory_LearningId]
        ON [History].[LearningHistory] ([LearningId]);
END
GO

------------------------------------------------------------
-- Move Episode FK from Learning → ApprenticeshipLearning
------------------------------------------------------------

-- 1. Drop old FK on Episode if it exists
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Episode_Learning'
      AND parent_object_id = OBJECT_ID('[dbo].[Episode]')
)
BEGIN
ALTER TABLE [dbo].[Episode]
DROP CONSTRAINT FK_Episode_Learning;
END
GO

-- 2. Drop old index on Episode if it exists
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_LearningKey'
      AND object_id = OBJECT_ID('[dbo].[Episode]')
)
BEGIN
DROP INDEX [IX_LearningKey]
    ON [dbo].[Episode];
END
GO

-- Now ensure the NEW FK + index exist on ApprenticeshipEpisode

-- 3. Create new FK if not already present
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ApprenticeshipEpisode_Learning'
      AND parent_object_id = OBJECT_ID('[dbo].[ApprenticeshipEpisode]')
)
BEGIN
ALTER TABLE [dbo].[ApprenticeshipEpisode]
    ADD CONSTRAINT FK_ApprenticeshipEpisode_Learning
    FOREIGN KEY (LearningKey)
    REFERENCES dbo.ApprenticeshipLearning ([Key]);
END
GO

-- 4. Create new index if not already present
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_LearningKey'
      AND object_id = OBJECT_ID('[dbo].[ApprenticeshipEpisode]')
)
BEGIN
CREATE INDEX IX_LearningKey
    ON [dbo].[ApprenticeshipEpisode] (LearningKey);
END
GO

------------------------------------------------------------
-- Move EpisodeBreakInLearning FK to ApprenticeshipEpisode
------------------------------------------------------------

-- 1. Drop old FK if it exists
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_EpisodeBreakInLearning_Episode'
      AND parent_object_id = OBJECT_ID('[dbo].[EpisodeBreakInLearning]')
)
BEGIN
ALTER TABLE dbo.EpisodeBreakInLearning
DROP CONSTRAINT FK_EpisodeBreakInLearning_Episode;
END
GO

-- 2. Create new FK if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_EpisodeBreakInLearning_ApprenticeshipEpisode'
      AND parent_object_id = OBJECT_ID('[dbo].[EpisodeBreakInLearning]')
)
BEGIN
ALTER TABLE dbo.EpisodeBreakInLearning
    ADD CONSTRAINT FK_EpisodeBreakInLearning_ApprenticeshipEpisode
        FOREIGN KEY (EpisodeKey)
            REFERENCES dbo.ApprenticeshipEpisode ([Key]);
END
GO


------------------------------------------------------------
-- Move EpisodePrice FK to ApprenticeshipEpisode
------------------------------------------------------------

-- 1. Drop old FK if it exists
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_EpisodePrice_Episode'
      AND parent_object_id = OBJECT_ID('[dbo].[EpisodePrice]')
)
BEGIN
ALTER TABLE dbo.EpisodePrice
DROP CONSTRAINT FK_EpisodePrice_Episode;
END
GO

-- 2. Drop old index if it exists
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_StartDateEndDate'
      AND object_id = OBJECT_ID('[dbo].[EpisodePrice]')
)
BEGIN
DROP INDEX [IX_StartDateEndDate]
    ON dbo.EpisodePrice;
END
GO

-- 3. Create new FK if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_EpisodePrice_ApprenticeshipEpisode'
      AND parent_object_id = OBJECT_ID('[dbo].[EpisodePrice]')
)
BEGIN
ALTER TABLE dbo.EpisodePrice
    ADD CONSTRAINT FK_EpisodePrice_ApprenticeshipEpisode
        FOREIGN KEY (EpisodeKey)
            REFERENCES dbo.ApprenticeshipEpisode ([Key]);
END
GO

-- 4. Recreate index if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_StartDateEndDate'
      AND object_id = OBJECT_ID('[dbo].[EpisodePrice]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_StartDateEndDate]
        ON dbo.EpisodePrice (EpisodeKey ASC, StartDate ASC, EndDate ASC);
END
GO


