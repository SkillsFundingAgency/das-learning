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