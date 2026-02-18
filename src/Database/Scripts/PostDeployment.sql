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

--Update Learning table with LearnerKey
UPDATE L
SET L.LearnerKey = LR.[Key]
FROM dbo.Learning AS L
    INNER JOIN dbo.Learner AS LR
ON L.Uln = LR.Uln
WHERE L.LearnerKey IS NULL;
