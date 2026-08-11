/*
 * FLP-1938 — EmployerType backfill
 * Run against Learning DB, das-lrn-db
 * Then execute OUTPUT against Earnings DB, das-funappern-db
 *
 * Purpose:
 *   Earnings Domain.ApprenticeshipEpisode.EmployerType is a new column that will default to 0 (NonLevy) for existing rows.
 *   This script sources the correct backfill value from the source-of-truth (Learning) and allows Earnings to be updated with correct values.
 *
 *   Only Levy rows need generating — Earnings EmployerType already defaults to NonLevy (0),
 *   which matches Learning's NonLevy rows, so nothing needs to change for them.
 *
 * Output:
 *   One UPDATE statement per Levy episode as text. Copy all rows from the output column and run
 *   them against the Earnings DB.
 *
 * Review before running generated script against Earnings: LevyEpisodeCount indicates the expected number of updated rows in Earnings.
  */

SET NOCOUNT ON;

-- Sanity check: number of levy apprenticeship episodes to update in Earnings.
SELECT COUNT(*) AS LevyEpisodeCount
FROM dbo.ApprenticeshipEpisode
WHERE EmployerType = 'Levy'

-- Generate one Earnings UPDATE statement per EpisodeKey to avoid cell-size truncation in output.
SELECT
    'UPDATE Domain.ApprenticeshipEpisode SET EmployerType = 1 WHERE [Key] = ' +
    QUOTENAME(CAST([Key] AS NVARCHAR(36)), '''') +
    ';' AS GeneratedUpdateScript
FROM dbo.ApprenticeshipEpisode
WHERE EmployerType = 'Levy'
ORDER BY [Key]
;