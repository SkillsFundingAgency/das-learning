/*
Post-deployment script
*/


--FLP-1493 - Data Migration

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
FROM Deduped
WHERE rn = 1;
GO

UPDATE L
SET L.LearnerKey = LR.[Key]
FROM dbo.Learning AS L
    INNER JOIN dbo.Learner AS LR
ON L.Uln = LR.Uln;
GO


