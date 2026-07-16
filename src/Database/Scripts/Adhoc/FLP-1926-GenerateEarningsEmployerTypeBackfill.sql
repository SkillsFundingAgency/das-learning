/*
 * FLP-1926 — EmployerType backfill
 * Run against Learning DB, das-lrn-db
 * Then execute OUTPUT against Earnings DB, das-funappern-db
 *
 * Purpose:
 *   Earnings Domain.ShortCourseEpisode.EmployerType is a new column that will default to 0 (NonLevy) for existing rows.
 *   This script sources the correct backfill value from the source-of-truth (Learning) and allows Earnings to be updated with correct values.
 *
 *   Only Levy rows need generating — Earnings EmployerType already defaults to NonLevy (0),
 *   which matches Learning's NonLevy rows, so nothing needs to change for them.
 *
 * Output:
 *   A single UPDATE statement as text. Copy it from the output column and run it against the
 *   Earnings DB.
 *
 * Review before running generated script against Earnings: LevyEpisodeCount indicates the expected number of updated rows in Earnings.
  */

SET NOCOUNT ON;

-- Sanity check: how many Levy episodes are we about to generate an update for?
SELECT COUNT(*) AS LevyEpisodeCount
FROM dbo.ShortCourseEpisode
WHERE EmployerType = 1; -- 1 = Levy

-- Generate the Earnings UPDATE statement, keyed on EpisodeKey.
SELECT
    'UPDATE Domain.ShortCourseEpisode' + CHAR(13) + CHAR(10) +
    'SET EmployerType = 1' + CHAR(13) + CHAR(10) +
    'WHERE [Key] IN (' + CHAR(13) + CHAR(10) +
    STRING_AGG(CAST(QUOTENAME(CAST([Key] AS NVARCHAR(36)), '''') AS NVARCHAR(MAX)), ',' + CHAR(13) + CHAR(10))
        WITHIN GROUP (ORDER BY [Key]) +
    CHAR(13) + CHAR(10) + ');' AS GeneratedUpdateScript
FROM dbo.ShortCourseEpisode
WHERE EmployerType = 1 -- 1 = Levy
;
