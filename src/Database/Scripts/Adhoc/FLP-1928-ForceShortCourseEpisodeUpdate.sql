/*
 * FLP-1928 - Force earnings re-sync for short course progression learners in prod
 * Run against Learning DB, das-lrn-db
 *
 * Purpose:
 * We previously fixed an issue where earnings were set incorrectly.
 * No earnings are fine, but if learning detects no changes in the incoming 
 * SLD payload compared to what it has persisted, it means that the outer api
 * does not bother to send an update to Earnings. This means that 
 * incorrect Earnings already persisted will never be updated.
 *
 * This script forces a detectable change to the Learning data, which will
 * trigger an update to Earnings.
 *
 * Output:
 *   - Number of impacted rows.
 */

SET NOCOUNT ON;

-- Sanity check: how many rows will be updated?
SELECT COUNT(*) AS RowsToUpdate
FROM dbo.ShortCourseEpisode
WHERE CompletionDate IS NOT NULL;

-- Apply one-day shift to force a detectable change.
UPDATE dbo.ShortCourseEpisode
SET CompletionDate = DATEADD(DAY, 1, CompletionDate)
WHERE CompletionDate IS NOT NULL;

-- Post-update check: show impacted row count.
SELECT @@ROWCOUNT AS RowsUpdated;
